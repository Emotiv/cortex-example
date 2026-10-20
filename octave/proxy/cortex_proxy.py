"""Line-delimited TCP <-> Cortex WSS bridge for MATLAB/Octave.

Requires: pip install websocket-client
Usage:    python cortex_proxy.py   (then run the Octave/MATLAB examples)
"""
import socket
import ssl
import threading
import json
import logging
from pathlib import Path

import websocket

TCP_HOST = "127.0.0.1"
TCP_PORT = 9999
CORTEX_URL = "wss://localhost:6868"
CORTEX_CONNECT_TIMEOUT = 10
CERTIFICATE_PATH = Path(__file__).resolve().parents[2] / "certificates" / "rootCA.pem"
SSL_OPTIONS = {"ca_certs": str(CERTIFICATE_PATH), "cert_reqs": ssl.CERT_REQUIRED}
SSL_OPTIONS["check_hostname"] = True
logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s %(levelname)s [%(threadName)s] %(message)s",
)
logger = logging.getLogger("cortex_proxy")


def send_line(conn, text):
    conn.sendall(text.replace("\r", "").replace("\n", "").encode("utf-8") + b"\n")


def wss_to_tcp(ws, conn):
    try:
        while True:
            message = ws.recv()
            if not message:
                logger.info("Cortex WebSocket closed its connection")
                break
            try:
                parsed = json.loads(message)
                summary = "id=%s keys=%s" % (parsed.get("id"), list(parsed.keys())) if isinstance(parsed, dict) else "JSON %s" % type(parsed).__name__
            except (TypeError, ValueError):
                summary = "non-JSON message"
            logger.info("WSS -> TCP: %d bytes (%s)", len(message), summary)
            send_line(conn, message)
    except Exception:
        logger.exception("WSS -> TCP forwarding failed")
    finally:
        try:
            conn.shutdown(socket.SHUT_RDWR)
        except OSError:
            pass


def tcp_to_wss(conn, ws):
    buffer = b""
    try:
        while True:
            chunk = conn.recv(65536)
            if not chunk:
                logger.info("TCP client closed its connection")
                break
            buffer += chunk
            while b"\n" in buffer:
                line, buffer = buffer.split(b"\n", 1)
                if line.strip():
                    text = line.decode("utf-8")
                    try:
                        request = json.loads(text)
                        logger.info("TCP -> WSS: method=%s id=%s", request.get("method"), request.get("id"))
                    except (TypeError, ValueError):
                        logger.warning("TCP -> WSS: forwarding non-JSON line (%d bytes)", len(line))
                    ws.send(text)
    except Exception:
        logger.exception("TCP -> WSS forwarding failed")


def require_certificate():
    if not CERTIFICATE_PATH.is_file():
        raise FileNotFoundError(
            "Required Cortex CA certificate not found: %s. "
            "Include the repository's certificates/rootCA.pem file when copying the Octave examples."
            % CERTIFICATE_PATH
        )


def handle_client(conn):
    conn.setsockopt(socket.IPPROTO_TCP, socket.TCP_NODELAY, 1)
    logger.info("TCP client connected from %s", conn.getpeername())
    try:
        ws = websocket.create_connection(
            CORTEX_URL,
            timeout=CORTEX_CONNECT_TIMEOUT,
            sslopt=SSL_OPTIONS,
        )
        ws.settimeout(None)
    except Exception as exc:
        logger.exception("Cannot connect to Cortex at %s", CORTEX_URL)
        try:
            send_line(conn, json.dumps({"status": "error", "message": str(exc)}))
        except OSError:
            logger.exception("Could not send connection error to TCP client")
        conn.close()
        return

    send_line(conn, '{"status": "connected"}')
    logger.info("WebSocket connected; bridge active to %s", CORTEX_URL)

    reader = threading.Thread(target=wss_to_tcp, args=(ws, conn), daemon=True)
    reader.start()
    tcp_to_wss(conn, ws)

    ws.close()
    conn.close()
    reader.join(timeout=2)
    logger.info("Bridge closed")


def main():
    require_certificate()
    server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    server.setsockopt(socket.IPPROTO_TCP, socket.TCP_NODELAY, 1)
    server.bind((TCP_HOST, TCP_PORT))
    server.listen(1)
    logger.info("Cortex proxy listening on %s:%d; upstream=%s", TCP_HOST, TCP_PORT, CORTEX_URL)
    try:
        while True:
            conn, _ = server.accept()
            try:
                handle_client(conn)
            except Exception:
                logger.exception("Unhandled client error")
            finally:
                conn.close()
    except KeyboardInterrupt:
        logger.info("Shutdown requested")
        pass
    finally:
        server.close()


if __name__ == "__main__":
    main()

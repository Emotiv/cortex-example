package cortex;

import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.security.KeyStore;
import java.security.cert.Certificate;
import java.security.cert.CertificateFactory;
import java.util.Collection;
import java.util.concurrent.BlockingQueue;
import java.util.concurrent.LinkedBlockingQueue;
import java.util.concurrent.TimeUnit;

import javax.net.ssl.SSLContext;
import javax.net.ssl.TrustManagerFactory;

import com.neovisionaries.ws.client.WebSocket;
import com.neovisionaries.ws.client.WebSocketAdapter;
import com.neovisionaries.ws.client.WebSocketFactory;
import com.neovisionaries.ws.client.WebSocketFrame;
import com.neovisionaries.ws.client.WebSocketState;

/** Polling WebSocket client for Octave/MATLAB, which cannot receive Java callbacks. */
public class CortexWsClient {
    private final BlockingQueue<String> messages = new LinkedBlockingQueue<String>();
    private volatile boolean closed = true;
    private WebSocket socket;

    /** An empty caPath uses the JVM default trust store. */
    public void connect(String url, String caPath, int timeoutMs) throws Exception {
        WebSocketFactory factory = new WebSocketFactory();
        factory.setConnectionTimeout(timeoutMs);
        factory.setSocketTimeout(timeoutMs);
        factory.setVerifyHostname(true);
        if (caPath != null && !caPath.isEmpty()) {
            factory.setSSLContext(sslContextFor(caPath));
        }

        socket = factory.createSocket(url);
        socket.addListener(new WebSocketAdapter() {
            @Override
            public void onTextMessage(WebSocket ws, String text) {
                messages.add(text);
            }

            @Override
            public void onStateChanged(WebSocket ws, WebSocketState newState) throws Exception {
                // Runs before the reader thread starts, so idle reads must block again.
                if (newState == WebSocketState.OPEN) {
                    ws.getSocket().setSoTimeout(0);
                }
            }

            @Override
            public void onDisconnected(WebSocket ws, WebSocketFrame serverFrame,
                    WebSocketFrame clientFrame, boolean closedByServer) {
                closed = true;
            }
        });

        closed = false;
        try {
            socket.connect();
        } catch (Exception e) {
            closed = true;
            throw e;
        }
    }

    public void sendText(String text) {
        if (closed) {
            throw new IllegalStateException("Connection to Cortex closed.");
        }
        socket.sendText(text);
    }

    /** Returns null on timeout; throws once the link is closed and the queue is drained. */
    public String poll(int timeoutMs) throws Exception {
        String message = messages.poll(timeoutMs, TimeUnit.MILLISECONDS);
        if (message == null && closed) {
            throw new IOException("Connection to Cortex closed.");
        }
        return message;
    }

    public boolean isOpen() {
        return !closed;
    }

    public void close() {
        closed = true;
        if (socket != null) {
            socket.disconnect();
        }
    }

    private static SSLContext sslContextFor(String caPath) throws Exception {
        KeyStore trustStore = KeyStore.getInstance(KeyStore.getDefaultType());
        trustStore.load(null, null);
        CertificateFactory certificateFactory = CertificateFactory.getInstance("X.509");
        try (InputStream in = new FileInputStream(caPath)) {
            Collection<? extends Certificate> certificates = certificateFactory.generateCertificates(in);
            int index = 0;
            for (Certificate certificate : certificates) {
                trustStore.setCertificateEntry("ca" + index++, certificate);
            }
        }

        TrustManagerFactory trustManagers = TrustManagerFactory.getInstance(TrustManagerFactory.getDefaultAlgorithm());
        trustManagers.init(trustStore);
        SSLContext context = SSLContext.getInstance("TLS");
        context.init(null, trustManagers.getTrustManagers(), null);
        return context;
    }
}

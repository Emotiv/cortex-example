import asyncio
from lib.cortex import Cortex


async def collect_data(cortex):
    # await cortex.inspectApi()
    print("** USER LOGIN **")
    await cortex.get_user_login()
    print("** GET CORTEX INFO **")
    await cortex.get_cortex_info()
    print("** HAS ACCESS RIGHT **")
    await cortex.has_access_right()
    print("** REQUEST ACCESS **")
    await cortex.request_access()
    print("** AUTHORIZE **")
    await cortex.authorize()
    print("** GET LICENSE INFO **")
    await cortex.get_license_info()
    print("** QUERY HEADSETS **")
    await cortex.query_headsets()
    if len(cortex.headsets) > 0:
        print("** CREATE SESSION **")
        await cortex.create_session(activate=True,
                                    headset_id=cortex.headsets[0])
        print("** CREATE RECORD **")
        await cortex.create_record(title="test record 1")
        print("** SUBSCRIBE POW & MET **")
        await cortex.subscribe(['pow'])
        #await cortex.subscribe(['dev', 'mot'])
        #await cortex.subscribe(['mot'])
        while cortex.packet_count < 10:
            await cortex.get_data()
        #await cortex.save_csv(suffix='one',label='F')
        await cortex.save_csv()
        await cortex.close_session()



def run():
    cortex = Cortex('./cortex_creds')
    asyncio.run(collect_data(cortex))
    cortex.close()


if __name__ == '__main__':
    run()

from src.httpClients import PassCountExport

if __name__ == '__main__':
    client = PassCountExport("https://vss-exports-stg.s3.us-west-2.amazonaws.com/3dpm/58950/test.zip?X-Amz-Expires=604800&X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=AKIAIBGOEETXHMANDX7A/20181122/us-west-2/s3/aws4_request&X-Amz-Date=20181122T235954Z&X-Amz-SignedHeaders=host&X-Amz-Signature=460ebedf647e3b73421f06fbe09963169c337bd3b49542a96173b44ef33a7eb4")
    report = client.get_passcount_export()
    for k in report.keys():
        print(k)
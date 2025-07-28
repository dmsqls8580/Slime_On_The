import pandas as pd
import json

EXCEL_PATH = './Assets/11_Data/Excel/SlimeTextData.xlsx'       
JSON_PATH  = './Assets/11_Data/Json/SlimeText.json' 
CSV_PATH   = './Assets/11_Data/CSV/SlimeTextDataCsv.csv'

df = pd.read_excel(EXCEL_PATH)

data = {"messages": df.to_dict(orient='records')}

with open(JSON_PATH, 'w', encoding='utf-8') as f:
    json.dump(data, f, ensure_ascii=False, indent=2)
print(f"✅ 변환 완료: {JSON_PATH}")

df.to_csv(CSV_PATH, index=False, encoding='utf-8-sig')
print(f"✅ CSV 변환 완료: {CSV_PATH}")
using Newtonsoft.Json.Linq;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEditor;
using UnityEngine;

public class EditorWindow_ExcelToJson : EditorWindow
{
    string Import_Excel_Folder = "_Tables/";
    string Export_Class_Folder = "_Export/Class/";
    string Export_Json_Folder = "_Export/Json/";
    string Folder_DataClass = "Assets/Scripts/Project_Portfolio/DataManager/";
    string Folder_Json = "Assets/Resources/GameData/";
    string FileName_DataClass_ByTables = "DataTableManager.cs";
    string FileName_JsonConverter = "DataTableLoader.cs";
    string FileName_Extension = ".json";
    string NameSpeceName = "Tables";
    bool bGen = false;
    int status_Gen = 0;

    [MenuItem("Window/Ex/00.Gen_ExcelToJson")]
    public static void ShowWindow()
    {
        GetWindow<EditorWindow_ExcelToJson>("ExcelToJson");
    }

    private void OnGUI()
    {
        GUILayout.Label("Path Setting", EditorStyles.boldLabel);
        Import_Excel_Folder = EditorGUILayout.TextField("Import Path : ", Import_Excel_Folder);
        Export_Class_Folder = EditorGUILayout.TextField("Export Class Path : ", Export_Class_Folder);
        Export_Json_Folder = EditorGUILayout.TextField("Export Json Path : ", Export_Json_Folder);
        GUILayout.Label("Name Setting", EditorStyles.boldLabel);
        NameSpeceName = EditorGUILayout.TextField("Export Namespace Name : ", NameSpeceName);
        FileName_DataClass_ByTables = EditorGUILayout.TextField("Export Data Class Name : ", FileName_DataClass_ByTables);
        FileName_JsonConverter = EditorGUILayout.TextField("Export Load Class Name  : ", FileName_JsonConverter);


        if (GUILayout.Button("Export ExcelToJson "))
        {
            Debug.Log("Button was Pressed");
            FolderInit();
            Clear_Export();
            Debug.Log("Start Convert");
            ProcessExporter_ExcelToClass();
            Debug.Log("End Convert");
        }

        GUILayout.Label("Capy Path Setting", EditorStyles.boldLabel);
        Folder_DataClass = EditorGUILayout.TextField("Class File Path : ", Folder_DataClass);
        Folder_Json = EditorGUILayout.TextField("Json File Path : ", Folder_Json);

        if (GUILayout.Button("Import Project GenFiles "))
        {
            Debug.Log("Button was Pressed");
            FolderInit();
            CapyData();
        }

        if (bGen)
            GUILayout.Label("Gen...", EditorStyles.boldLabel);
        else
        {
            switch (status_Gen)
            {
                case 0:
                    GUILayout.Label("Status", EditorStyles.boldLabel);
                    break;
                case 1:
                    GUILayout.Label("<color=#00ff00ff>Suc</color>", EditorStyles.boldLabel);
                    break;
                default:
                    GUILayout.Label("<color=#ff0000ff>Fail</color>", EditorStyles.boldLabel);
                    break;
            }
        }
    }
    void CapyData()
    {
        string[] class_files = Directory.GetFiles(Folder_DataClass);
        string[] json_files = Directory.GetFiles(Folder_Json);

        foreach (var e in class_files)
        {
            Debug.Log($"Delete Class File : {e}");
            File.Delete(e);
        }
        foreach (var e in json_files)
        {
            //Debug.Log($"Delete Json File : {e}");
            //File.Delete(e);
        }
        string[] class_files_c = Directory.GetFiles(Export_Class_Folder);
        foreach (var e in class_files_c)
        {
            Debug.Log($"Copy From File : {e}");
            string filename = e.Replace(Export_Class_Folder, "");
            Debug.Log($"Copy to File : {Folder_DataClass}{filename}");
            File.Copy(e, $"{Folder_DataClass}{filename}");
        }
        string[] json_files_c = Directory.GetFiles(Export_Json_Folder);
        foreach (var e in json_files_c)
        {
            Debug.Log($"Copy From File : {e}");
            string filename = e.Replace(Export_Json_Folder, "");
            Debug.Log($"Copy to File : {Folder_Json}{filename}");
            File.Copy(e, $"{Folder_Json}{filename}", true);
        }
    }
    void FolderInit()
    {
        // Out Asset
        if (!Directory.Exists(Import_Excel_Folder))
        {
            Directory.CreateDirectory(Import_Excel_Folder);
            Debug.LogWarning($"Create Folder : {Import_Excel_Folder}");
        }
        if (!Directory.Exists(Export_Class_Folder))
        {
            Directory.CreateDirectory(Export_Class_Folder);
            Debug.LogWarning($"Create Folder : {Export_Class_Folder}");
        }
        if (!Directory.Exists(Export_Json_Folder))
        {
            Directory.CreateDirectory(Export_Json_Folder);
            Debug.LogWarning($"Create Folder : {Export_Json_Folder}");
        }

        // In Asset
        if (!Directory.Exists(Folder_DataClass))
        {
            Directory.CreateDirectory(Folder_DataClass);
            Debug.LogWarning($"Create Folder : {Folder_DataClass}");
        }
        if (!Directory.Exists(Folder_Json))
        {
            Directory.CreateDirectory(Folder_Json);
            Debug.LogWarning($"Create Folder : {Folder_Json}");
        }
    }
    void Clear_Export()
    {
        string[] class_files = Directory.GetFiles(Export_Class_Folder);
        string[] json_files = Directory.GetFiles(Export_Json_Folder);

        foreach (var e in class_files)
        {
            Debug.Log($"Delete Class File : {e}");
            File.Delete(e);
        }
        foreach (var e in json_files)
        {
            Debug.Log($"Delete Json File : {e}");
            File.Delete(e);
        }
    }

    void ProcessExporter_ExcelToClass(bool bPretty = true)
    {
        if (!Directory.Exists(Import_Excel_Folder))
        {
            Debug.LogError($"NotExisist TableDataFolder : {Import_Excel_Folder}");
            return;
        }


        ArrayList FileList = new ArrayList();
        if (!GetTableList(Import_Excel_Folder, FileList))
            return;

        //폴더가 없으면 만든다.
        if (!Directory.Exists(Export_Class_Folder))
            Directory.CreateDirectory(Export_Class_Folder);

        IWorkbook workbook = null;
        ISheet sheet = null;
        Stream file_DataClass = File.Open($"{Export_Class_Folder}{FileName_DataClass_ByTables}", FileMode.Create);
        StreamWriter swriter_DataClass = new StreamWriter(file_DataClass, Encoding.UTF8);

        WriteBeginFile(swriter_DataClass, NameSpeceName);

        ArrayList arrAnn = new ArrayList();
        ArrayList arrName = new ArrayList();
        ArrayList arrDel = new ArrayList();
        ArrayList arrType = new ArrayList();

        List<string> classNames = new List<string>();
        List<string> idxNames = new List<string>();
        List<object> types = new List<object>();

        for (int i = 0; i < FileList.Count; ++i)
        {
            workbook = null;
            sheet = null;
            arrAnn.Clear();
            arrName.Clear();
            arrDel.Clear();
            arrType.Clear();

            string className = FileList[i].ToString().Substring(0, FileList[i].ToString().IndexOf(".xlsx"));

            ExcelLoad(ref workbook, ref sheet, string.Concat(Import_Excel_Folder, FileList[i].ToString()));

            if (!SettingSeparator(workbook, sheet, arrAnn, arrName, arrDel, arrType))
                continue;

            classNames.Add(className);
            idxNames.Add(arrName[0].ToString());
            types.Add(arrType[0]);
            // write, 파일에 쓰기
            WriteMemberData(swriter_DataClass, arrType, arrName, arrDel, arrAnn, className);

            Type typeClass = ClassGen(className, arrName, arrDel, arrType);
            if (typeClass == null)
            {
                Debug.LogError($"{className}클래스 생성 실패");
                swriter_DataClass.Close();
                file_DataClass.Close();
                return;
            }

            //테이블 정보를 불러와 Setting
            ArrayList list_tdata = SettingDataValue(sheet, typeClass, arrName, arrDel, arrType);

            //파일 열기
            Stream file_DataToJson = File.Open(string.Concat(Export_Json_Folder, className, FileName_Extension), FileMode.Create);
            StreamWriter swriter_DataToJson = new StreamWriter(file_DataToJson, Encoding.UTF8);

            // json 추출
            // 컨버터 사용 X
            swriter_DataToJson.Write("{\n");
            swriter_DataToJson.Write("    \"name\" : \"" + className + "\",\n");
            swriter_DataToJson.Write("    \"value\" : {\n");
            //swriter_DataToJson.Write("{\"Items\":");




            for (int j = 0; j < list_tdata.Count; ++j)
            {
                JObject job = JObject.Parse(JsonUtility.ToJson(list_tdata[j], bPretty));


                if (bPretty)
                    if (j != 0)
                        swriter_DataToJson.WriteLine(",");
                    else
                        if (j != 0)
                        swriter_DataToJson.Write(",");


                swriter_DataToJson.Write("    \"" + job["key"] + "\" : ");
                swriter_DataToJson.Write(job.ToString() + "\n");
            }
            swriter_DataToJson.Write("\n  }\n}");

            // 파일 닫기
            swriter_DataToJson.Close();
            file_DataToJson.Close();
        }

        WriteEndFile(swriter_DataClass);

        swriter_DataClass.Close();
        file_DataClass.Close();

        WriteJsonConverter(classNames, types, idxNames);

        FileList.Clear();
    }
    Type ClassGen(string className, ArrayList arrName, ArrayList arrDel, ArrayList arrType)
    {
        //현재 어플리케이션 도매인을 가져온다.
        AppDomain currentDomain = AppDomain.CurrentDomain;
        //생성하려는 어셈블리의 이름을 설정한다.
        AssemblyName assemName = new AssemblyName($"DA_{className}");
        //어셈블리를 생성
        AssemblyBuilder assemBuilder = AssemblyBuilder.DefineDynamicAssembly(assemName, AssemblyBuilderAccess.Run);
        //모듈 생성
        ModuleBuilder moduleBuilder = assemBuilder.DefineDynamicModule($"DM_{className}");
        //클래스 생성 
        TypeBuilder typeBuilder = moduleBuilder.DefineType($"DC_{className}", TypeAttributes.Public | TypeAttributes.Serializable);
        //필드 생성
        for (int i = 0; i < arrName.Count; ++i)
        {
            if (arrDel[i].Equals(true))
                continue;
            EDataType dt = GetDataType(arrType[i].ToString());
            FieldBuilder field;
            PropertyBuilder property;
            MethodBuilder method_g, method_s;
            ILGenerator e_g, e_s;
            Type tp = null;
            OpCode op = new OpCode();
            bool suc = true;
            switch (dt)
            {
                case EDataType.Byte:
                    tp = typeof(byte);
                    op = OpCodes.Stind_I1;
                    break;
                case EDataType.Bool:
                    tp = typeof(bool);
                    op = OpCodes.Stind_I4;
                    break;
                case EDataType.UInt:
                    tp = typeof(uint);
                    op = OpCodes.Stind_I4;
                    break;
                case EDataType.Int:
                    tp = typeof(int);
                    op = OpCodes.Stind_I4;
                    break;
                case EDataType.ULong:
                    tp = typeof(ulong);
                    op = OpCodes.Stind_I8;
                    break;
                case EDataType.Long:
                    tp = typeof(long);
                    op = OpCodes.Stind_I8;
                    break;
                case EDataType.Float:
                    tp = typeof(float);
                    op = OpCodes.Stind_R4;
                    break;
                case EDataType.Double:
                    tp = typeof(double);
                    op = OpCodes.Stind_R8;
                    break;
                case EDataType.String:
                    tp = typeof(string);
                    op = OpCodes.Stind_Ref;
                    break;
                case EDataType.Arr_Bool:
                    tp = typeof(bool[]);
                    op = OpCodes.Stind_Ref;
                    break;
                case EDataType.Arr_Int:
                    tp = typeof(int[]);
                    op = OpCodes.Stind_Ref;
                    break;
                case EDataType.Arr_Float:
                    tp = typeof(float[]);
                    op = OpCodes.Stind_Ref;
                    break;
                case EDataType.Arr_Double:
                    tp = typeof(double[]);
                    op = OpCodes.Stind_Ref;
                    break;
                case EDataType.Arr_String:
                    tp = typeof(string[]);
                    op = OpCodes.Stind_Ref;
                    break;
                default:
                    suc = false;
                    break;
            }
            if (suc)
            {
                field = typeBuilder.DefineField(arrName[i].ToString(), tp, FieldAttributes.Public);
                property = typeBuilder.DefineProperty(arrName[i].ToString(), PropertyAttributes.None, tp, Type.EmptyTypes);
                method_g = typeBuilder.DefineMethod($"get_{arrName[i].ToString()}", MethodAttributes.Public | MethodAttributes.SpecialName, tp, Type.EmptyTypes);
                e_g = method_g.GetILGenerator();
                e_g.Emit(OpCodes.Ldarg_0);
                e_g.Emit(OpCodes.Ldfld, field);
                e_g.Emit(OpCodes.Ret);
                property.SetGetMethod(method_g);

                method_s = typeBuilder.DefineMethod($"Set{arrName[i].ToString()}", MethodAttributes.Private | MethodAttributes.SpecialName, typeof(void), new Type[] { tp });
                e_s = method_s.GetILGenerator();
                e_s.Emit(OpCodes.Ldarg_0);
                e_s.Emit(OpCodes.Ldflda, field);
                e_s.Emit(OpCodes.Ldarg_1);
                e_s.Emit(op);
                e_s.Emit(OpCodes.Ret);
                property.SetSetMethod(method_s);
            }
        }
        return typeBuilder.CreateType();
    }
    void WriteJsonConverter(List<string> names, List<object> keyType, List<string> idxNames)
    {
        if (names == null)
            return;
        Stream file_Data = File.Open(Export_Class_Folder + FileName_JsonConverter, FileMode.Create);
        StreamWriter sw = new StreamWriter(file_Data, Encoding.UTF8);
        string className = FileName_JsonConverter.Substring(0, FileName_JsonConverter.IndexOf(".cs"));
        string tab = "\t";
        string path_DataFolder = Folder_Json.Replace("Assets/", "");
        string[] paths = Folder_Json.Split('/');
        string name_DataFolder = Folder_Json.Substring(("Assets/Resources/").Length);
        // sw.WriteLine("using System;");
        // sw.WriteLine("using System.Linq;");

        sw.WriteLine("using UnityEngine;");
        sw.WriteLine("using System;");
        sw.WriteLine("using System.IO;");
        sw.WriteLine("using System.Collections.Generic;");
        sw.WriteLine("using Newtonsoft.Json.Linq;");
        sw.WriteLine();
        sw.WriteLine($"public static class {className}");
        sw.WriteLine("{");
        sw.WriteLine($"{tab}public static bool Loaded {{ get; set; }} = false;");
        sw.WriteLine($"{tab}/// <summary>");
        sw.WriteLine($"{tab}/// tables 을 로딩한다.");
        sw.WriteLine($"{tab}/// </summary");
        sw.WriteLine($"{tab}public static void Load()");
        sw.WriteLine($"{tab}{{");
        //sw.WriteLine($"{tab}{tab}if (Loaded)");
        //sw.WriteLine($"{tab}{tab}{tab}return;");
        sw.WriteLine($"#if UNITY_EDITOR");
        sw.WriteLine($"{tab}{tab}List<TextAsset> txts = new List<TextAsset>();");
        sw.WriteLine($"{tab}{tab}DirectoryInfo di = new DirectoryInfo(Path.Combine(Application.dataPath, \"{path_DataFolder}\"));");
        sw.WriteLine($"{tab}{tab}FileInfo[] fileInfo = di.GetFiles();");
        sw.WriteLine($"{tab}{tab}for (int i = 0; i < fileInfo.Length; i++)");
        sw.WriteLine($"{tab}{tab}{{");
        sw.WriteLine($"{tab}{tab}{tab}if (fileInfo[i].Extension.CompareTo(\".json\") != 0)");
        sw.WriteLine($"{tab}{tab}{tab}{tab}continue;");
        sw.WriteLine($"{tab}{tab}{tab}TextAsset txt = new TextAsset(fileInfo[i].OpenText().ReadToEnd());");
        sw.WriteLine($"{tab}{tab}{tab}txt.name = fileInfo[i].Name.Replace(\".json\", \"\");");
        sw.WriteLine($"{tab}{tab}{tab}txts.Add(txt);");
        sw.WriteLine($"{tab}{tab}}}");
        sw.WriteLine($"#else");
        sw.WriteLine($"{tab}{tab}TextAsset[] txts = Resources.LoadAll<TextAsset>(\"{name_DataFolder}\");");
        sw.WriteLine($"#endif");
        sw.WriteLine($"{tab}{tab}foreach (TextAsset e in txts)");
        sw.WriteLine($"{tab}{tab}{tab}FromJsonConvert(e);");
        sw.WriteLine($"{tab}{tab}Loaded = true;");
        sw.WriteLine($"{tab}}}");

        sw.WriteLine($"{tab}[Serializable]");
        sw.WriteLine($"{tab}private class Wrapper<T>");
        sw.WriteLine($"{tab}{{");
        sw.WriteLine($"{tab}{tab}public T[] Items = null;");
        sw.WriteLine($"{tab}}}");

        sw.WriteLine($"{tab}public static T[] FromJson<T>(string json)");
        sw.WriteLine($"{tab}{{");
        sw.WriteLine($"{tab}{tab}Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);");
        sw.WriteLine($"{tab}{tab}return wrapper.Items;");
        sw.WriteLine($"{tab}}}");

        sw.WriteLine(string.Concat(tab, "public static void FromJsonConvert(TextAsset txt)"));
        sw.WriteLine(string.Concat(tab, "{"));
        sw.WriteLine(string.Concat(tab, tab, "switch (txt.name)"));
        sw.WriteLine(string.Concat(tab, tab, "{"));
        for (int i = 0; i < names.Count; ++i)
        {
            sw.WriteLine(string.Concat(tab, tab, tab, "case \"", names[i], "\":"));
            //sw.WriteLine(string.Concat(tab, tab, tab, tab, $"foreach (var e in FromJson<{NameSpeceName}.{names[i]}>(txt.text))"));
            //sw.WriteLine(string.Concat(tab, tab, tab, tab, "{"));
            //sw.WriteLine(string.Concat(tab, tab, tab, tab, tab, $"if(!{NameSpeceName}.{names[i]}.data.ContainsKey(e.{idxNames[i]}))"));
            //sw.WriteLine(string.Concat(tab, tab, tab, tab, tab, tab, $"{NameSpeceName}.{names[i]}.data.Add(e.{idxNames[i]}, e);"));
            //sw.WriteLine(string.Concat(tab, tab, tab, tab, tab, "else"));
            //sw.WriteLine(string.Concat(tab, tab, tab, tab, tab, tab, $"Debug.LogError(string.Concat(\"TableName : \", txt.name,\" Duplicate(중복된) Index : \",e.{idxNames[i]}.ToString()));"));
            //sw.WriteLine(string.Concat(tab, tab, tab, tab, "}"));
            sw.WriteLine(string.Concat(tab, tab, tab, tab, "{"));
            sw.WriteLine(string.Concat(tab, tab, tab, tab, tab, "JObject json = JObject.Parse(txt.text);"));
            sw.WriteLine(string.Concat(tab, tab, tab, tab, tab, $"{NameSpeceName}.{names[i]}.data = json.GetDeserializedObject(\"value\", new Dictionary<{keyType[i]}, {NameSpeceName}.{names[i]}>());"));
            sw.WriteLine(string.Concat(tab, tab, tab, tab, tab, "Debug.Log(\"", names[i], " is loaded\");"));
            sw.WriteLine(string.Concat(tab, tab, tab, tab, "}"));
            sw.WriteLine(string.Concat(tab, tab, tab, tab, "break;"));
        }
        sw.WriteLine(string.Concat(tab, tab, tab, "default:"));
        sw.WriteLine(string.Concat(tab, tab, tab, tab, "Debug.Log(string.Concat(\"Not Fount equal txt.name Data : \", txt.name));"));
        sw.WriteLine(string.Concat(tab, tab, tab, tab, "break;"));
        sw.WriteLine(string.Concat(tab, tab, "}"));
        sw.WriteLine(string.Concat(tab, "}"));
        sw.WriteLine("}");

        sw.Close();
        file_Data.Close();
    }
    bool SettingSeparator(IWorkbook workbook, ISheet sheet, ArrayList arrAnn, ArrayList arrName, ArrayList arrDel, ArrayList arrType)
    {
        if (workbook != null && sheet != null)
        {
            for (int row = 0; row < 4; ++row)
            {
                if (sheet.GetRow(row) == null)
                    continue;
                for (int col = 0; col < sheet.GetRow(row).LastCellNum; ++col)
                {
                    switch (row)
                    {
                        case 0: // annotation(주석)
                            arrAnn.Add(sheet.GetRow(row).GetCell(col));
                            break;
                        case 1: // class member name(멤버명)
                            arrName.Add(sheet.GetRow(row).GetCell(col));
                            break;
                        case 2: // use?(사용여부)
                            if (sheet.GetRow(row).GetCell(col) == null)
                                arrDel.Add(false);
                            else
                            {
                                string strValue = sheet.GetRow(row).GetCell(col).ToString();
                                if (strValue.Contains("x") || strValue.Contains("X"))
                                    arrDel.Add(true);
                                else
                                    arrDel.Add(false);
                            }
                            break;
                        case 3: // data type(데이터 종류)
                            arrType.Add(sheet.GetRow(row).GetCell(col));
                            break;
                    }
                }
            }
            if (arrType.Count > arrDel.Count)
            {
                int addCnt = arrType.Count - arrDel.Count;
                for (int j = 0; j < addCnt; ++j)
                {
                    arrDel.Add(false);
                }
            }
        }
        else
            return false;
        return true;
    }
    ArrayList SettingDataValue(ISheet sheet, Type typeClass, ArrayList arrName, ArrayList arrDel, ArrayList arrType)
    {
        ArrayList list_tdata = new ArrayList();

        for (int row = 4; row <= sheet.LastRowNum; ++row)
        {
            // class 인스턴스 생성
            object tdata = Activator.CreateInstance(typeClass);
            int col = 0;
            try
            {
                for (; col < sheet.GetRow(row).LastCellNum; ++col)
                {
                    if (arrDel[col].Equals(true))
                        continue;
                    IRow Row = sheet.GetRow(row);
                    EDataType tGT = GetDataType(arrType[col].ToString());
                    PropertyInfo data = typeClass.GetProperty(arrName[col].ToString());
                    switch (tGT)
                    {
                        case EDataType.Byte:
                            data.SetValue(tdata, Convert.ToByte(Row.GetCell(col).ToString(), 16));
                            break;
                        case EDataType.Bool:
                            if (Row.GetCell(col) == null)
                                data.SetValue(tdata, false);
                            else if (Row.GetCell(col).ToString().Length == 1)
                            {
                                if (GetValue<int>(Row.GetCell(col).ToString()) == 0)
                                    data.SetValue(tdata, false);
                                else
                                    data.SetValue(tdata, true);
                            }
                            else
                                data.SetValue(tdata, GetValue<bool>(Row.GetCell(col).ToString()));
                            break;
                        case EDataType.UInt:
                            data.SetValue(tdata, GetValue<uint>(Row.GetCell(col).ToString()));
                            break;
                        case EDataType.Int:
                            if (Row.GetCell(col) == null)
                                data.SetValue(tdata, 0);
                            else if (Row.GetCell(col).CellType == NPOI.SS.UserModel.CellType.Formula)
                                data.SetValue(tdata, (int)Row.GetCell(col).NumericCellValue);
                            else
                                data.SetValue(tdata, GetValue<int>(Row.GetCell(col).ToString()));
                            break;
                        case EDataType.ULong:
                            data.SetValue(tdata, GetValue<ulong>(Row.GetCell(col).ToString()));
                            break;
                        case EDataType.Long:
                            data.SetValue(tdata, GetValue<long>(Row.GetCell(col).ToString()));
                            break;
                        case EDataType.Float:
                            if (Row.GetCell(col) == null)
                                data.SetValue(tdata, 0.0f);
                            else if (Row.GetCell(col).CellType == NPOI.SS.UserModel.CellType.Formula)
                                data.SetValue(tdata, (float)Row.GetCell(col).NumericCellValue);
                            else
                                data.SetValue(tdata, GetValue<float>(Row.GetCell(col).ToString()));
                            break;
                        case EDataType.Double:
                            if (Row.GetCell(col) == null)
                                data.SetValue(tdata, 0.0);
                            else if (Row.GetCell(col).CellType == NPOI.SS.UserModel.CellType.Formula)
                                data.SetValue(tdata, Row.GetCell(col).NumericCellValue);
                            else
                                data.SetValue(tdata, GetValue<double>(Row.GetCell(col).ToString()));
                            break;
                        case EDataType.String:
                            if (Row.GetCell(col) != null)
                                data.SetValue(tdata, GetValue<string>(Row.GetCell(col).ToString()));
                            else
                                data.SetValue(tdata, string.Empty);
                            break;
                        case EDataType.Arr_Int:
                            if (Row.GetCell(col) != null)
                            {
                                string[] info;
                                if (Row.GetCell(col).CellType == NPOI.SS.UserModel.CellType.Formula)
                                    info = Row.GetCell(col).StringCellValue.Split(',');
                                else
                                    info = Row.GetCell(col).ToString().Split(',');
                                int[] info_i = new int[info.Length];
                                for (int i = 0; i < info.Length; ++i)
                                {
                                    if (info[i].Equals(string.Empty))
                                        continue;
                                    if (!int.TryParse(info[i], out info_i[i]))
                                    {
#if UNITY_EDITOR
                                        Debug.LogError($"{typeClass} Not Current DataType {tGT} {info[i]}");
#endif
                                    }
                                }
                                data.SetValue(tdata, info_i);
                            }
                            else
                                data.SetValue(tdata, GetValue<int>(Row.GetCell(col).ToString()));
                            break;

                        case EDataType.Arr_Float:
                            if (Row.GetCell(col) != null)
                            {
                                string[] info;
                                if (Row.GetCell(col).CellType == NPOI.SS.UserModel.CellType.Formula)
                                    info = Row.GetCell(col).StringCellValue.Split(',');
                                else
                                    info = Row.GetCell(col).ToString().Split(',');
                                float[] info_i = new float[info.Length];
                                for (int i = 0; i < info.Length; ++i)
                                {
                                    if (info[i].Equals(string.Empty))
                                        continue;
                                    if (!float.TryParse(info[i], out info_i[i]))
                                    {
#if UNITY_EDITOR
                                        Debug.LogError($"{typeClass} Not Current DataType {tGT} {info[i]}");
#endif
                                    }
                                }
                                data.SetValue(tdata, info_i);
                            }
                            else
                                data.SetValue(tdata, GetValue<float>(Row.GetCell(col).ToString()));
                            break;

                        case EDataType.Arr_String:
                            if (Row.GetCell(col) != null)
                            {
                                string[] info;
                                if (Row.GetCell(col).CellType == NPOI.SS.UserModel.CellType.Formula)
                                    info = Row.GetCell(col).StringCellValue.Split(',');
                                else
                                    info = Row.GetCell(col).ToString().Split(',');
                                data.SetValue(tdata, info);
                            }
                            else
                                data.SetValue(tdata, GetValue<int>(Row.GetCell(col).ToString()));
                            break;
                        case EDataType.Arr_Double:
                            if (Row.GetCell(col) != null)
                            {
                                string[] info;
                                if (Row.GetCell(col).CellType == NPOI.SS.UserModel.CellType.Formula)
                                    info = Row.GetCell(col).StringCellValue.Split(',');
                                else
                                    info = Row.GetCell(col).ToString().Split(',');
                                double[] info_i = new double[info.Length];
                                for (int i = 0; i < info.Length; ++i)
                                {
                                    if (info[i].Equals(string.Empty))
                                        continue;
                                    if (!double.TryParse(info[i], out info_i[i]))
                                    {
#if UNITY_EDITOR
                                        Debug.LogError($"{typeClass} Not Current DataType {tGT} {info[i]}");
#endif
                                    }
                                }
                                data.SetValue(tdata, info_i);
                            }
                            else
                                data.SetValue(tdata, GetValue<float>(Row.GetCell(col).ToString()));
                            break;
                        case EDataType.None:
                        default:
                            Debug.LogError($"nameList[{col}]{arrName[col].ToString()} is not data type - {arrType[col]}");
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                IRow Row = sheet.GetRow(row);
                IRow tmpRow = sheet.GetRow(1);
                Debug.LogError($"{typeClass} : row:{row} - {Row.GetCell(0)} , col:{col} - {tmpRow.GetCell(col)} \n {e}");
                break;
            }
            var tmp = Convert.ChangeType(tdata, typeClass);
            list_tdata.Add(tmp);
        }
        return list_tdata;
    }
    void ExcelLoad(ref IWorkbook workbook, ref ISheet sheet, string path, string sheetName = "")
    {
        try
        {
            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                string extension = GetExtension(path);

                if (extension == "xls")
                {
                    workbook = new HSSFWorkbook(fileStream);
                }
                else if (extension == "xlsx")
                {
                    workbook = new XSSFWorkbook(fileStream);
                }
                else
                {
                    throw new Exception("Wrong file.");
                }
                //string[] sheetNames = GetSheetNames();
                //Debug.Log("sheetNames Length : " + sheetNames.Length + " = " + sheetNames[0]);

                if (string.IsNullOrEmpty(sheetName))
                    sheet = workbook.GetSheetAt(0);
                else
                    sheet = workbook.GetSheet(sheetName);
                //Debug.Log("LastRowNum : " + sheet.LastRowNum);
            }
        }
        catch (Exception e)
        {
            Debug.Log(path);
            Debug.LogError(e.Message);
        }
    }
    string GetExtension(string path)
    {
        string ext = Path.GetExtension(path);
        string[] arg = ext.Split(new char[] { '.' });

        return arg[1];
    }
    /// <summary>
    /// table list 가져오기
    /// </summary>
    bool GetTableList(string path, ArrayList list)
    {
        DirectoryInfo dir = new DirectoryInfo(path);

        FileInfo[] file = dir.GetFiles();
        if (file.Length == 0)
            return false;

        for (int i = 0; i < file.Length; ++i)
        {
            if (file[i].FullName.LastIndexOf(".xlsx") > 0 &&
                file[i].FullName.LastIndexOf(".meta") < 0)
                list.Add(file[i].Name);
        }
        if (file.Length == 0)
            return false;
        else
            return true;
    }
    /// <summary>
    /// sw 파일에 begin token 부분까지 복사한다.
    /// </summary>
    void WriteBeginFile(StreamWriter sw, string spaceName)
    {

        sw.WriteLine("using UnityEngine;");
        sw.WriteLine("using System.Collections.Generic;");
        sw.WriteLine();
        sw.WriteLine(string.Concat("namespace ", spaceName));
        sw.WriteLine("{");
        sw.WriteLine();
    }
    /// <summary>
    /// sw 파일에 end token 부분까지 나올때까지 돌고 그 이후 부분을 복사한다.
    /// </summary>
    void WriteEndFile(StreamWriter sw)
    {
        sw.WriteLine("}");
    }
    /// <summary>
    /// 파일에 class member 를 채운다.
    /// </summary>
    void WriteMemberData(StreamWriter sw, ArrayList arrType, ArrayList arrName, ArrayList arrDel, ArrayList arrAnn, string className, bool bInterface = true)
    {
        //sw.WriteLine("\t[System.Serializable]");
        sw.WriteLine("\tpublic partial class " + className);
        sw.WriteLine("\t{");
        string tab;
        for (int i = 0; i < arrType.Count; ++i)
        {
            //if(i < arrDel.Count)
            if (arrDel[i].Equals(true)) // x 인건 뺀다.
                continue;

            // type
            if (arrType[i].ToString().Length < 4)
                tab = "\t\t\t";
            else
                tab = "\t\t";

            // 주석
            if (i < arrAnn.Count)
                sw.WriteLine("\t\t/// <summary> " + arrAnn[i] + " </summary>");
            else
                sw.WriteLine("\t\t/// <summary></summary>");

            // 멤버
            if (arrType[i].ToString().IndexOf("arr_") >= 0)
            {
                string[] szType = arrType[i].ToString().Split('_');
                if (szType[1].ToString().Length < 4)
                    tab = "\t\t\t";
                else
                    tab = "\t\t";

                sw.WriteLine($"\t\t[Newtonsoft.Json.JsonProperty] public {szType[1]}{tab}[] {arrName[i]} {{get; private set;}}");
            }
            else
            {
                sw.WriteLine(string.Concat("\t\t[Newtonsoft.Json.JsonProperty] public ", arrType[i], tab, arrName[i], " {get; private set;}"));
            }
        }

        if (bInterface)
        {
            sw.WriteLine();
            // data Dictionary. 메인 저장소
            sw.WriteLine("\t\t// 메인 저장소");
            sw.WriteLine("\t\tpublic static Dictionary<" + arrType[0] + ", " + className + "> data = new Dictionary<" + arrType[0] + ", " + className + ">();");

            // get data(int)
            sw.WriteLine("\t\tpublic static " + className + " Get(" + arrType[0] + " key)");
            sw.WriteLine("\t\t{\r\n\t\t\tif (data.ContainsKey(key))\r\n\t\t\t\treturn data[key];\r\n\t\t\telse\r\n\t\t\t{\r\n\t\t\t\tUnityEngine.Debug.LogWarningFormat(\"This Key doesn't exist in " + className + " Table Key : {0}\",key);\r\n\t\t\t\treturn null;\r\n\t\t\t}\r\n\t\t}");
        }
        // class end
        sw.WriteLine("\t}");
        sw.WriteLine();
    }
    EDataType GetDataType(string str)
    {
        if (str.Equals("byte"))
            return EDataType.Byte;
        else if (str.Equals("bool"))
            return EDataType.Bool;
        else if (str.Equals("uint"))
            return EDataType.UInt;
        else if (str.Equals("int"))
            return EDataType.Int;
        else if (str.Equals("ulong"))
            return EDataType.ULong;
        else if (str.Equals("long"))
            return EDataType.Long;
        else if (str.Equals("float"))
            return EDataType.Float;
        else if (str.Equals("double"))
            return EDataType.Double;
        else if (str.Equals("string"))
            return EDataType.String;
        else if (str.Equals("arr_bool"))
            return EDataType.Arr_Bool;
        else if (str.Equals("arr_int"))
            return EDataType.Arr_Int;
        else if (str.Equals("arr_float"))
            return EDataType.Arr_Float;
        else if (str.Equals("arr_double"))
            return EDataType.Arr_Double;
        else if (str.Equals("arr_string"))
            return EDataType.Arr_String;
        else
            return EDataType.None;
    }
    /// <summary>
    /// 문자열을 해당 type 값으로 변경
    /// 값이 비어있으면 0 or "" 으로
    /// </summary>
    T GetValue<T>(string str)
    {
        //DebugX.Log(typeof(T).ToString());
        if (str.Equals(""))
        {
            if (typeof(T).ToString() == typeof(string).ToString())
                return (T)Convert.ChangeType("", typeof(T));
            else
                return (T)Convert.ChangeType(0, typeof(T));
        }
        return (T)Convert.ChangeType(str, typeof(T));
    }
    public enum EDataType
    {
        None = 0,
        Byte,
        Bool,
        UInt,
        Int,
        ULong,
        Long,
        Float,
        Double,
        String,
        Arr_Bool,
        Arr_Int,
        Arr_Float,
        Arr_Double,
        Arr_String,
    }
}
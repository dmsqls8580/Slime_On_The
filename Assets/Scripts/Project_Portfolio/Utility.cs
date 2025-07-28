using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using NPOI.Util;


public static class Utility
{
    public static string[] CurrencyUnits = new string[] { "", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };

    public static T GetValue<T>(this JObject jObj, string key, T defaultValue = default) where T : IComparable, IConvertible, IEquatable<T>
    {
        try
        {
            if (jObj.TryGetValue(key, out JToken token))
                return token.ToObject<T>();
        }
        catch
        {

        }

        return defaultValue;
    }

    public static T GetDeserializedObject<T>(this JObject jObj, string key, T defaultValue = default)
    {
        if (jObj[key] == null)
        {
            return defaultValue;
        }

        return JsonConvert.DeserializeObject<T>(jObj[key].ToString());
    }

    public static void RemoveChildAll(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child != null)
                GameObject.Destroy(child.gameObject);
        }
    }

    public static string ToCurrencyString(double _number)
    {
        string zero = "0";
        if (-1d < _number && _number < 1d)  // 0일 때
            return zero;

        if (double.IsInfinity(_number))   // 무한 일 때 
            return "Infinity";
        //  부호 출력 문자열
        string significant = (_number < 0) ? "-" : string.Empty;
        //  보여줄 숫자
        string showNumber = string.Empty;
        //  단위 문자열
        string unityString = string.Empty;
        //  패턴을 단순화 시키기 위해 무조건 지수 표현식으로 변경한 후 처리
        string[] partsSplit = _number.ToString("E").Split('+');
        //  예외
        if (partsSplit.Length < 2)
            return zero;
        //  지수 (자릿수 표현)
        if (!int.TryParse(partsSplit[1], out int exponent))  // 지수가 존재하는지
        {
            Debug.LogWarningFormat("Failed - ToCurrentString({0}) : partSplit[1] = {1}", _number, partsSplit[1]);
            return zero;
        }
        //  몫은 문자열 인덱스
        int quotient = exponent / 3;   // 000이 3개 , 1000 단위로 값이 변경된다
        //  나머지는 정수부 자릿수 계산에 사용(10의 거듭제곱을 사용)
        int remainder = exponent % 3;
        //  1A 미만은 그냥 표현
        if (exponent < 3)
            showNumber = System.Math.Truncate(_number).ToString();
        else
        {
            //  10의 거듭제곱을 구해서 자릿수 표현값을 만들어 준다.
            double temp = double.Parse(partsSplit[0].Replace("E", "")) * System.Math.Pow(10, remainder);
            //  소수 둘째자리까지만 출력한다.
            showNumber = temp.ToString("F").Replace(".00", "");
        }
        unityString = CurrencyUnits[quotient];
        return string.Format("{0}{1}{2}", significant, showNumber, unityString);
    }
}

using System;
public enum TimeType
{
    //年月日
    YEAR_MONTH_DAY,
    //时分秒
    HOUR_MIN_SEC,
    //时--分
    HOUR_MIN,
    //分——秒
    MIN_SEC,
    //秒
    SEC,
}


public class ToolBox {
    
    /// <summary>
    /// 通过秒获取时间的字符串
    /// </summary>
    /// <param name="sec"></param>
    /// <returns></returns>
    public static string GetTimeStrBySec (int sec, TimeType timeType) {
        string finalTimeStr = string.Empty;
        string hourTimeStr = string.Empty;
        string minTimeStr = string.Empty;
        string secTimeStr = string.Empty;

        //秒字符串
        int secTime = sec % 60;
        if (secTime < 10) {
            secTimeStr = "0" + secTime;
        } else {
            secTimeStr = secTime.ToString ();
        }

        //分字符串
        int minTime = sec / 60;
        if (minTime < 10) {
            minTimeStr = "0" + minTime;
        } else {
            minTimeStr = minTime.ToString ();
        }

        //小时字符串
        int hourTime = sec / 3600;
        if (hourTime < 10) {
            hourTimeStr = "0" + hourTime;
        } else {
            hourTimeStr = hourTime.ToString ();
        }

        switch (timeType) {
            case TimeType.HOUR_MIN_SEC:
                finalTimeStr = hourTimeStr + ":" + minTimeStr + ":" + secTimeStr;
                break;
            case TimeType.HOUR_MIN:
                finalTimeStr = hourTimeStr + ":" + minTimeStr;
                break;
            case TimeType.MIN_SEC:
                finalTimeStr = minTimeStr + ":" + secTimeStr;
                break;
            case TimeType.SEC:
                finalTimeStr = secTimeStr;
                break;
            default:
                break;
        }
        return finalTimeStr;
    }

    /// <summary>
    /// 根据时间戳获取时间
    /// </summary>
    /// <param name="timeStamp"></param>
    /// <param name="timeType"></param>
    /// <returns></returns>
    public static string GetTimeStrByTimestamp (long timeStamp, TimeType timeType) {
        string finalTimeStr = string.Empty;
        System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime (new System.DateTime (1970, 1, 1)); // 当地时区
        DateTime dt = startTime.AddSeconds (timeStamp);
        switch (timeType) {
            case TimeType.YEAR_MONTH_DAY:
                finalTimeStr = dt.ToString ("yyyy-MM-dd");
                break;
            case TimeType.HOUR_MIN_SEC:
                finalTimeStr = dt.Hour + ":" + dt.Minute + ":" + dt.Second;
                break;
            case TimeType.HOUR_MIN:
                finalTimeStr = dt.Hour + ":" + dt.Minute;
                break;
            case TimeType.MIN_SEC:
                finalTimeStr = dt.Minute + ":" + dt.Second;
                break;
            case TimeType.SEC:
                finalTimeStr = dt.Second.ToString ();
                break;
            default:
                break;
        }
        return finalTimeStr;
    }
}
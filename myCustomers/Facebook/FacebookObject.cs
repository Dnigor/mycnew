using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Script.Serialization;

namespace myCustomers.Facebook
{
    public class FacebookObject
    {
        // private ctor forces callers to use static factory method CreateFromString
        FacebookObject() { }

        public FacebookObject[] Array { get { return _arrayData; } }

        public bool Boolean { get { return Convert.ToBoolean(_stringData); } }

        public Dictionary<string, FacebookObject> Dictionary { get { return _dictData; } }

        public Int64 Integer { get { return Convert.ToInt64(_stringData); } }

        public bool IsArray { get { return _arrayData != null; } }

        public bool IsBoolean
        {
            get
            {
                bool tmp;
                return bool.TryParse(_stringData, out tmp);
            }
        }

        public bool IsDictionary { get { return _dictData != null; } }

        public bool IsInteger
        {
            get
            {
                Int64 tmp;
                return Int64.TryParse(_stringData, out tmp);
            }
        }

        public bool IsString { get { return _stringData != null; } }

        public string String { get { return _stringData; } }

        public string ToDisplayableString()
        {
            var sb = new StringBuilder();
            RecursiveObjectToString(this, sb, 0);
            return sb.ToString();
        }

        public static FacebookObject CreateFromString(string s)
        {
            object o;

            var js = new JavaScriptSerializer();
            try
            {
                o = js.DeserializeObject(s);
            }
            catch (ArgumentException)
            {
                throw new Exception("Not a valid JSON string.");
            }

            return Create(o);
        }

        static FacebookObject Create(object o)
        {
            FacebookObject obj = new FacebookObject();
            if (o is object[])
            {
                var objArray = o as object[];
                obj._arrayData = new FacebookObject[objArray.Length];
                for (var i = 0; i < obj._arrayData.Length; ++i)
                {
                    obj._arrayData[i] = Create(objArray[i]);
                }
            }
            else if (o is Dictionary<string, object>)
            {
                obj._dictData = new Dictionary<string, FacebookObject>();
                var dict = o as Dictionary<string, object>;
                foreach (var key in dict.Keys)
                {
                    obj._dictData[key] = Create(dict[key]);
                }
            }
            else if (o != null) // o is a scalar
            {
                obj._stringData = o.ToString();
            }

            return obj;
        }

        static void RecursiveDictionaryToString(FacebookObject obj, StringBuilder sb, int level)
        {
            foreach (var kvp in obj.Dictionary)
            {
                sb.Append(' ', level * 2);
                sb.Append(kvp.Key);
                sb.Append(" => ");
                RecursiveObjectToString(kvp.Value, sb, level);
                sb.AppendLine();
            }
        }

        static void RecursiveObjectToString(FacebookObject obj, StringBuilder sb, int level)
        {
            if (obj.IsDictionary)
            {
                sb.AppendLine();
                RecursiveDictionaryToString(obj, sb, level + 1);
            }
            else if (obj.IsArray)
            {
                foreach (FacebookObject o in obj.Array)
                {
                    RecursiveObjectToString(o, sb, level);
                    sb.AppendLine();
                }
            }
            else // some sort of scalar value
            {
                sb.Append(obj.String);
            }
        }

        FacebookObject[] _arrayData;
        Dictionary<string, FacebookObject> _dictData;
        string _stringData;
    }
}

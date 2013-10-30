using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace myCustomers.Data
{
    public abstract class BaseGenerator<T>
    {
        public bool IsFunctional { get; protected set; }

        public abstract T GeneratedInstance(DataRow row);

        public virtual List<T> GeneratedList(DataTable table)
        {
            List<T> list;

            if (table != null)
            {
                list = new List<T>(table.Rows.Count);
                foreach (DataRow row in table.Rows)
                {
                    T instance = GeneratedInstance(row);
                    if (instance != null)
                    {
                        list.Add(instance);
                    }
                }
            }
            else
            {
                list = new List<T>(0);
            }

            return list;
        }


        protected class ValueGenerator<TValue>
        {
            public ValueGenerator(DataColumnCollection columns, XElement xeConfiguration)
                : this(GetColumnOrdinals(columns, xeConfiguration), GetAcceptedPattern(xeConfiguration), GetParseFormats(xeConfiguration))
            {
            }

            public ValueGenerator(DataColumnCollection columns, XElement xeConfiguration, string childElementName)
                : this(GetColumnOrdinals(columns, xeConfiguration, childElementName), GetAcceptedPattern(xeConfiguration, childElementName), GetParseFormats(xeConfiguration, childElementName))
            {
            }

            protected ValueGenerator(IList<int> columnOrdinals, Regex acceptedPattern, IList<string> parseFormats)
            {
                ColumnOrdinals  = (columnOrdinals != null) ? new List<int>(columnOrdinals) : new List<int>(0);
                AcceptedPattern = acceptedPattern;
                ParseFormats    = (parseFormats != null) ? new List<string>(parseFormats) : new List<string>(0);
            }


            public TValue DefaultValue { get; set; }

            public bool IsFunctional { get { return (ColumnOrdinals.Count > 0); } }

            public TValue Value(DataRow row)
            {
                TValue value = Value(row, DefaultValue);
                return value;
            }

            public TValue Value(DataRow row, TValue defaultValue)
            {
                TValue value = defaultValue;
                foreach (int columnOrdinal in ColumnOrdinals)
                {
                    object columnValue = row[columnOrdinal];
                    if (columnValue != null)
                    {
                        if (AcceptedPattern != null)
                        {
                            if ((columnValue is string) && AcceptedPattern.IsMatch(columnValue as string))
                            {
                                value = ParsedValue(columnValue);
                            }
                        }
                        else
                        {
                            value = ParsedValue(columnValue);
                        }
                        break;
                    }
                }
                return value;
            }

            protected Regex AcceptedPattern { get; set; }

            protected List<int> ColumnOrdinals { get; set; }

            protected List<string> ParseFormats { get; set; }

            protected static Regex GetAcceptedPattern(XElement xeConfiguration)
            {
                Regex acceptedPattern = null;

                if (xeConfiguration != null)
                {
                    string acceptedPatternValue = xeConfiguration.AttributeValue("AcceptedPattern", false);
                    if (!string.IsNullOrWhiteSpace(acceptedPatternValue))
                    {
                        acceptedPattern = new Regex(acceptedPatternValue);
                    }
                }

                return acceptedPattern;
            }

            protected static Regex GetAcceptedPattern(XElement xeConfiguration, string childElementName)
            {
                Regex acceptedPattern = null;

                if ((xeConfiguration != null) && !string.IsNullOrWhiteSpace(childElementName))
                {
                    XElement xeChild = xeConfiguration.Element(childElementName);
                    if (xeChild != null)
                    {
                        acceptedPattern = GetAcceptedPattern(xeChild);
                    }
                }

                return acceptedPattern;
            }

            protected static List<int> GetColumnOrdinals(DataColumnCollection columns, IList<string> columnNames)
            {
                List<int> columnOrdinals = null;

                if (columnNames != null)
                {
                    columnOrdinals = new List<int>(columnNames.Count);
                    foreach (string columnName in columnNames)
                    {
                        if (!string.IsNullOrWhiteSpace(columnName) && columns.Contains(columnName))
                        {
                            columnOrdinals.Add(columns[columnName].Ordinal);
                        }
                    }
                }

                return columnOrdinals;
            }

            protected static List<int> GetColumnOrdinals(DataColumnCollection columns, XElement xeConfiguration)
            {
                List<int> columnOrdinals = null;

                if (xeConfiguration != null)
                {
                    var columnNamesValue = xeConfiguration.AttributeValue("ColumnNames", false);
                    var columnNames      = !string.IsNullOrWhiteSpace(columnNamesValue) ? columnNamesValue.Split(',') : new string[] { };
                    columnOrdinals       = GetColumnOrdinals(columns, columnNames);
                }

                return columnOrdinals;
            }

            protected static List<int> GetColumnOrdinals(DataColumnCollection columns, XElement xeConfiguration, string childElementName)
            {
                List<int> columnOrdinals = null;

                if ((xeConfiguration != null) && !string.IsNullOrWhiteSpace(childElementName))
                {
                    XElement xeChild = xeConfiguration.Element(childElementName);
                    if (xeChild != null)
                    {
                        columnOrdinals = GetColumnOrdinals(columns, xeChild);
                    }
                }

                return columnOrdinals;
            }

            protected static string[] GetParseFormats(XElement xeConfiguration)
            {
                string[] parseFormats = null;

                if (xeConfiguration != null)
                {
                    string parseFormatsValue = xeConfiguration.AttributeValue("ParseFormats", false);
                    parseFormats = !string.IsNullOrWhiteSpace(parseFormatsValue) ? parseFormatsValue.Split('|') : new string[] { };
                }

                return parseFormats;
            }

            protected static string[] GetParseFormats(XElement xeConfiguration, string childElementName)
            {
                string[] parseFormats = null;

                if ((xeConfiguration != null) && !string.IsNullOrWhiteSpace(childElementName))
                {
                    XElement xeChild = xeConfiguration.Element(childElementName);
                    if (xeChild != null)
                    {
                        parseFormats = GetParseFormats(xeChild);
                    }
                }

                return parseFormats;
            }

            protected virtual TValue ParsedValue(object columnValue)
            {
                return (TValue)columnValue;
            }
        }

        protected class DateTimeValueGenerator : ValueGenerator<DateTime?>
        {
            public DateTimeValueGenerator(DataColumnCollection columns, XElement xeConfiguration)
                : base(columns, xeConfiguration)
            {
                DefaultValue = null;
            }

            public DateTimeValueGenerator(DataColumnCollection columns, XElement xeConfiguration, string childElementName)
                : base(columns, xeConfiguration, childElementName)
            {
                DefaultValue = null;
            }

            public CultureInfo ParseCulture { get; set; }

            public DateTimeStyles? ParseStyles { get; set; }

            protected override DateTime? ParsedValue(object columnValue)
            {
                DateTime? parsedValue = null;
                if (columnValue is DateTime)
                {
                    parsedValue = (DateTime)columnValue;
                }
                else if (columnValue is DateTime?)
                {
                    parsedValue = (DateTime?)columnValue;
                }
                else if ((columnValue is string) && (ParseFormats != null))
                {
                    string columnValueText = columnValue as string;
                    CultureInfo culture = (ParseCulture != null) ? ParseCulture : CultureInfo.CurrentUICulture;
                    DateTimeStyles styles = (ParseStyles != null) ? ParseStyles.Value : DateTimeStyles.AdjustToUniversal | DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal;
                    DateTime value;
                    if (DateTime.TryParseExact(columnValueText, ParseFormats.ToArray(), culture, styles, out value))
                    {
                        parsedValue = value;
                    }
                }
                return parsedValue;
            }

        }

        protected class StringValueGenerator : ValueGenerator<string>
        {
            public StringValueGenerator(DataColumnCollection columns, XElement xeConfiguration)
                : base(columns, xeConfiguration)
            {
                DefaultValue = null;
            }

            public StringValueGenerator(DataColumnCollection columns, XElement xeConfiguration, string childElementName)
                : base(columns, xeConfiguration, childElementName)
            {
                DefaultValue = null;
            }

            protected override string ParsedValue(object columnValue)
            {
                return columnValue.ToString();
            }
        }
    }
}

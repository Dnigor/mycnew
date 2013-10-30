using System;
using System.Collections.Generic;
using System.Data;

namespace myCustomers.Data
{
	public class CsvParser
	{
		public CsvParser(string content)
		{
			Content = content;
			ContentLength = (content != null) ? content.Length : 0;
			Index = 0;
			Table = new DataTable();
			List<string> columnNames = ParsedRecord(-1);
			foreach (string columnName in columnNames)
			{
				Table.Columns.Add(columnName, typeof(string));
			}
			ParseRecords();
		}

		public CsvParser(string content, IEnumerable<string> columnNames)
		{
			if (columnNames == null) throw new ArgumentNullException("columnNames");

			Content = content;
			ContentLength = (content != null) ? content.Length : 0;
			Index = 0;
			Table = new DataTable();
			foreach (string columnName in columnNames)
			{
				Table.Columns.Add(columnName, typeof(string));
			}
			ParseRecords();
		}

		public DataTable Table { get; private set; }

		protected List<string> ColumnNames { get; set; }

		protected string Content { get; set; }

		protected int ContentLength { get; set; }

		protected int Index { get; set; }

		protected bool IsEndOfFile { get { return (Index > ContentLength); } }

		protected bool IsEndOfRecord { get; set; }

		protected string CurrentEntity()
		{
			string currentEntity;
			switch (ContentLength - Index)
			{
				case -1:
					currentEntity = null;
					break;

				case 0:
					currentEntity = "";
					break;

				case 1:
					currentEntity = Content.Substring(Index, 1);
					break;

				default:
					string start = Content.Substring(Index, 2);
					if (start.StartsWith("\r\n"))
					{
						currentEntity = "\r\n";
					}
					else
					{
						currentEntity = start.Substring(0, 1);
					}
					break;
			}
			return currentEntity;
		}

		protected static void GetFirstMatch(string source, int startIndex, string[] values, out int? matchIndex, out string matchValue)
		{
			matchIndex = null;
			matchValue = null;
			foreach (string value in values)
			{
				int index = source.IndexOf(value, startIndex);
				if ((index >= startIndex) && (!matchIndex.HasValue || (index < matchIndex.Value)))
				{
					matchIndex = index;
					matchValue = value;
				}
			}
		}

		protected void ParseRecords()
		{
			int columns = Table.Columns.Count;
			while (!IsEndOfFile)
			{
				List<string> parsedFields = ParsedRecord(columns);
				DataRow parsedRow = Table.NewRow();
				for (int columnIndex = 0; columnIndex < columns; columnIndex += 1)
				{
					if (columnIndex < parsedFields.Count)
					{
						parsedRow[columnIndex] = parsedFields[columnIndex];
					}
					else
					{
						parsedRow[columnIndex] = null;
					}
				}
				Table.Rows.Add(parsedRow);
			}
		}

		protected string ParsedField()
		{
			string parsedField = null;

			string startEntity = CurrentEntity();
			switch (startEntity)
			{
				case null:
					IsEndOfRecord = true;
					break;

				case "":
					Index += 1;
					IsEndOfRecord = true;
					break;

				case ",":
					Index += startEntity.Length;
					break;

				case "\n":
				case "\r":
				case "\r\n":
					Index += startEntity.Length;
					IsEndOfRecord = true;
					break;

				case "\"":
					{
						bool isEndFound = false;
						int nextIndex = Index + 1;
						do
						{
							int quoteIndex = Content.IndexOf('"', nextIndex);
							if (quoteIndex >= nextIndex)
							{
								if (Content.IndexOf("\"\"", quoteIndex) == quoteIndex)
								{
									nextIndex = quoteIndex + 2;
								}
								else
								{
									parsedField = Content.Substring(Index + 1, quoteIndex - Index - 1).Replace("\"\"", "\"");
									Index = quoteIndex + 1;
									isEndFound = true;
								}
							}
							else
							{
								throw new InvalidOperationException("Unable to find closing double quote of field.");
							}
						}
						while (!isEndFound);

						string nextEntity = CurrentEntity();
						switch (nextEntity)
						{
							case null:
								IsEndOfRecord = true;
								break;

							case "":
								Index += 1;
								IsEndOfRecord = true;
								break;

							case ",":
								Index += nextEntity.Length;
								break;

							case "\n":
							case "\r":
							case "\r\n":
								Index += nextEntity.Length;
								IsEndOfRecord = true;
								break;

							default:
								throw new InvalidOperationException("Unexpected character encountered following closing double quote of field.");
						}
					}
					break;

				default:
					string[] values = new string[] { ",", "\r\n", "\n", "\r" };
					int? matchIndex;
					string matchValue;
					GetFirstMatch(Content, Index + 1, values, out matchIndex, out matchValue);
					switch (matchValue)
					{
						case ",":
							parsedField = Content.Substring(Index, matchIndex.Value - Index);
							Index = matchIndex.Value + matchValue.Length;
							break;

						case "\n":
						case "\r":
						case "\r\n":
							parsedField = Content.Substring(Index, matchIndex.Value - Index);
							Index = matchIndex.Value + matchValue.Length;
							IsEndOfRecord = true;
							break;

						default:
							parsedField = Content.Substring(Index);
							Index = ContentLength + 1;
							IsEndOfRecord = true;
							break;
					}
					break;
			}

			if (string.IsNullOrEmpty(parsedField)) parsedField = null;

			return parsedField;
		}

		protected List<string> ParsedRecord(int capacity)
		{
			List<string> parsedFields = (capacity >= 0) ? new List<string>(capacity) : new List<string>();
			if (!IsEndOfFile)
			{
				IsEndOfRecord = false;
				while (!IsEndOfRecord)
				{
					string parsedField = ParsedField();
					parsedFields.Add(parsedField);
				}
			}
			return parsedFields;
		}
	}
}

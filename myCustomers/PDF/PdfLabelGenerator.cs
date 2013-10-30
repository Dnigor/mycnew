using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace myCustomers.Pdf
{
	public class PdfLabelGenerator : PDFGenerator
	{
		public PdfLabelGenerator(MailingLabelDS data) : this(data, 10f) { }

		public PdfLabelGenerator(MailingLabelDS data, float fontsize)
		{
			_ds = data;
			_times = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
			_timesfont = new Font(_times, fontsize, Font.NORMAL);
		}

		public PdfLabelGenerator(XElement config, MailingLabelDS data, string baseFontIncludingFullPath)
			: this(config, data, baseFontIncludingFullPath, 10f)
		{
			_config = config;
		}

		public PdfLabelGenerator(XElement config, MailingLabelDS data, string baseFontIncludingFullPath, float fontSize)
		{
			_config = config;
			_ds = data;
			if (string.IsNullOrEmpty(baseFontIncludingFullPath))
			{
				_times = BaseFont.CreateFont("Times-Roman", "windows-1252", BaseFont.NOT_EMBEDDED);
			}
			else
			{
				_times = BaseFont.CreateFont(baseFontIncludingFullPath, "Identity-H", BaseFont.EMBEDDED);
			}
			_timesfont = new Font(_times, fontSize, 0);
		}

		public int Border
		{
			get
			{
				if (!_border.HasValue)
				{
					if (_config != null)
					{
						var borderName = GetAttributeValue<string>(_config, "Border");
						if (!string.IsNullOrEmpty(borderName))
						{
							var borderField = typeof(Rectangle).GetField(borderName, BindingFlags.Static | BindingFlags.Public);
							if (borderField != null)
							{
								_border = borderField.GetValue(null) as int?;
							}
						}
					}

					if (!_border.HasValue)
					{
						_border = Rectangle.NO_BORDER;
					}
				}
				return _border.Value;
			}
		}

		public float BottomMargin
		{
			get
			{
				if (!_bottomMargin.HasValue)
				{
					if (_config != null)
					{
						_bottomMargin = GetAttributeValue<float>(_config, "BottomMargin");
					}
					else
					{
						_bottomMargin = 0f;
					}
				}
				return _bottomMargin.Value;
			}
		}

		public string ConfigName
		{
			get { return _configName; }
			set
			{
				if (_configName != value)
				{
					_configName = value;
					_config = null;
				}
			}
		}

		public float GutterWidth
		{
			get
			{
				if (!_gutterWidth.HasValue)
				{
					if (_config != null)
					{
						_gutterWidth = GetAttributeValue<float>(_config, "GutterWidth");
					}
					else
					{
						_gutterWidth = 1.5f;
					}
				}
				return _gutterWidth.Value;
			}
		}

		public int HorizontalAlignment
		{
			get
			{
				if (!_horizontalAlignment.HasValue)
				{
					if (_config != null)
					{
						string horizontalAlignmentName = GetAttributeValue<string>(_config, "HorizontalAlignment");
						if (!string.IsNullOrEmpty(horizontalAlignmentName))
						{
							FieldInfo horizontalAlignmentField = typeof(Element).GetField(horizontalAlignmentName, BindingFlags.Static | BindingFlags.Public);
							if (horizontalAlignmentField != null)
							{
								_horizontalAlignment = horizontalAlignmentField.GetValue(null) as int?;
							}
						}
					}

					if (!_horizontalAlignment.HasValue)
					{
						_horizontalAlignment = Rectangle.NO_BORDER;
					}
				}
				return _horizontalAlignment.Value;
			}
		}

		public int HorizontalLabelCount
		{
			get
			{
				if (!_horizontalLabelCount.HasValue)
				{
					if (_config != null)
					{
						_horizontalLabelCount = GetAttributeValue<int>(_config, "HorizontalLabelCount");
					}
					else
					{
						_horizontalLabelCount = 3;
					}
				}
				return _horizontalLabelCount.Value;
			}
		}

		public float LabelWidth
		{
			get
			{
				if (!_lableWidth.HasValue)
				{
					if (_config != null)
					{
						_lableWidth = GetAttributeValue<float>(_config, "LabelWidth");
					}
					else
					{
						_lableWidth = 32.3f;
					}
				}
				return _lableWidth.Value;
			}
		}

		public float LeftMargin
		{
			get
			{
				if (!_leftMargin.HasValue)
				{
					if (_config != null)
					{
						_leftMargin = GetAttributeValue<float>(_config, "LeftMargin");
					}
					else
					{
						_leftMargin = 13.68f;
					}
				}
				return _leftMargin.Value;
			}
		}

		public float MinimumHeight
		{
			get
			{
				if (!_minimumHeight.HasValue)
				{
					if (_config != null)
					{
						_minimumHeight = GetAttributeValue<float>(_config, "MinimumHeight");
					}
					else
					{
						_minimumHeight = 72f;
					}
				}
				return _minimumHeight.Value;
			}
		}

		public Rectangle PageSize
		{
			get
			{
				if (_pageSize == null)
				{
					if (_config != null)
					{
						_pageSize = iTextSharp.text.PageSize.GetRectangle(GetAttributeValue<string>(_config, "PageSize"));
					}
					else
					{
						_pageSize = iTextSharp.text.PageSize.LETTER;
					}
				}
				return _pageSize;
			}
		}

		public float PaddingLeft
		{
			get
			{
				if (!_paddingLeft.HasValue)
				{
					if (_config != null)
					{
						_paddingLeft = GetAttributeValue<float>(_config, "PaddingLeft");
					}
					else
					{
						_paddingLeft = 7f;
					}
				}
				return _paddingLeft.Value;
			}
		}

		public float RightMargin
		{
			get
			{
				if (!_rightMargin.HasValue)
				{
					if (_config != null)
					{
						_rightMargin = GetAttributeValue<float>(_config, "RightMargin");
					}
					else
					{
						_rightMargin = 13.68f;
					}
				}
				return _rightMargin.Value;
			}
		}

		public float TopMargin
		{
			get
			{
				if (!_topMargin.HasValue)
				{
					if (_config != null)
					{
						_topMargin = GetAttributeValue<float>(_config, "TopMargin");
					}
					else
					{
						_topMargin = 36f;
					}
				}
				return _topMargin.Value;
			}
		}

		public int VerticalAlignment
		{
			get
			{
				if (!_verticalAlignment.HasValue)
				{
					if (_config != null)
					{
						string VerticalAlignmentName = GetAttributeValue<string>(_config, "VerticalAlignment");
						if (!string.IsNullOrEmpty(VerticalAlignmentName))
						{
							FieldInfo verticalAlignmentField = typeof(Element).GetField(VerticalAlignmentName, BindingFlags.Static | BindingFlags.Public);
							if (verticalAlignmentField != null)
							{
								_verticalAlignment = verticalAlignmentField.GetValue(null) as int?;
							}
						}
					}

					if (!_verticalAlignment.HasValue)
					{
						_verticalAlignment = Rectangle.NO_BORDER;
					}
				}
				return _verticalAlignment.Value;
			}
		}

		public int VerticalLabelCount
		{
			get
			{
				if (!_verticalLabelCount.HasValue)
				{
					if (_config != null)
					{
						_verticalLabelCount = GetAttributeValue<int>(_config, "VerticalLabelCount");
					}
					else
					{
						_verticalLabelCount = 10;
					}
				}
				return _verticalLabelCount.Value;
			}
		}

		public float WidthPercentage
		{
			get
			{
				if (!_widthPercentage.HasValue)
				{
					if (_config != null)
					{
						_widthPercentage = GetAttributeValue<float>(_config, "WidthPercentage");
					}
					else
					{
						_widthPercentage = 100f;
					}
				}
				return _widthPercentage.Value;
			}
		}

		public override void SavePdf(Stream s)
		{
			if (LabelsPerPage > 0 && _ds.Label.Rows.Count > 0)
			{
				Document document = new Document(PageSize, LeftMargin, RightMargin, TopMargin, BottomMargin);
				PdfWriter writer = PdfWriter.GetInstance(document, s); // Required in order to render
				PdfPTable table = null;

				document.Open();

				foreach (MailingLabelDS.LabelRow row in _ds.Label)
				{
					// Check for a page break
					if (_ds.Label.Rows.IndexOf(row) % LabelsPerPage == 0)
					{
						// Add a completed page to the document
						if (table != null)
						{
						  document.Add(table);
						}
						document.NewPage();
						table = new PdfPTable(HorizontalLabelCount * 2 - 1);
						table.DefaultCell.Border = Border;
						table.DefaultCell.PaddingLeft = PaddingLeft;
						table.DefaultCell.HorizontalAlignment = HorizontalAlignment;
						table.DefaultCell.VerticalAlignment = VerticalAlignment;
						table.DefaultCell.MinimumHeight = MinimumHeight;
						table.SetWidths(ColumnWidths);
						table.WidthPercentage = WidthPercentage;
					}
					// Add a gutter cell unless this is the first Lable on a row
					if (_ds.Label.Rows.IndexOf(row) % HorizontalLabelCount != 0)
					{
						table.AddCell("");
					}
					// Add the Label
					AddCell(table, row);
				}

				// iTextSharp will not render a row unless it is complete... so add blank cells for last partial line
				for (int unusedLablesOnLastRowCount = (HorizontalLabelCount - (_ds.Label.Rows.Count % HorizontalLabelCount)) % HorizontalLabelCount; unusedLablesOnLastRowCount > 0; unusedLablesOnLastRowCount--)
				{
					// Gutter cell
					table.AddCell("");
					// Label cell
					table.AddCell("");
				}

				// Add partail page
				document.Add(table);
				document.Close();
			}
		}

		private float[] ColumnWidths
		{
			get
			{
				if (_columnWidths == null)
				{
					List<float> columnWidths = new List<float>();
					for (int i = 0; i < HorizontalLabelCount; i++)
					{
						if (i != 0)
						{
							columnWidths.Add(GutterWidth);
						}
						columnWidths.Add(LabelWidth);
					}
					_columnWidths = columnWidths.ToArray();
				}
				return _columnWidths;
			}
		}

		int LabelsPerPage { get { return HorizontalLabelCount * VerticalLabelCount; } }

		void AddCell(PdfPTable t, MailingLabelDS.LabelRow row)
		{
			var s = new StringBuilder();

			if (!row.IsLine1Null())
				AddLine(s, row.Line1, true);

            if (!row.IsLine2Null())
				AddLine(s, row.Line2, true);

            if (!row.IsLine3Null())
				AddLine(s, row.Line3, true);

            if (!row.IsLine4Null())
				AddLine(s, row.Line4, true);

            if (!row.IsLine5Null())
				AddLine(s, row.Line5, true);

            t.AddCell(new Phrase(s.ToString(), _timesfont));
		}

		void AddLine(StringBuilder s, string text, bool measureandshrink)
		{
			if (measureandshrink)
			{
				while (_times.GetWidthPoint(text, _timesfont.Size) > 175)
				{
					text = text.Substring(0, text.Length - 1);
				}
			}

			s.Append(text);
			s.Append("\n");
		}

		T GetAttributeValue<T>(XElement element, string attributeName)
		{
			T value = default(T);
			if (element != null && !string.IsNullOrEmpty(attributeName) && element.Attribute(attributeName) != null && !string.IsNullOrEmpty(element.Attribute(attributeName).Value))
			{
				value = (T)Convert.ChangeType(element.Attribute(attributeName).Value, typeof(T));
			}
			return value;
		}

		float?         _bottomMargin         = null;
		int?           _border               = null;
		float[]        _columnWidths         = null;
		XElement       _config               = null;
		string         _configName           = "LabelsFormat";
		MailingLabelDS _ds;
		float?         _gutterWidth          = null;
		int?           _horizontalAlignment  = null;
		int?           _horizontalLabelCount = null;
		float?         _lableWidth           = null;
		float?         _leftMargin           = null;
		float?         _minimumHeight        = null;
		float?         _paddingLeft          = null;
		Rectangle      _pageSize             = null;
		float?         _rightMargin          = null;
		BaseFont       _times;
		Font           _timesfont;
		float?         _topMargin            = null;
		int?           _verticalAlignment    = null;
		int?           _verticalLabelCount   = null;
		float?         _widthPercentage      = null;
	}
}

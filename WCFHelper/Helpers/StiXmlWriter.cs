using System;
using System.Text;
using System.Collections.Generic;
using System.Collections;
using Stimulsoft.Base.Drawing;

namespace WCFHelper
{
    public class StiXmlWriter
    {
        #region Fields
        public bool IsEncodeString = false;
        private StringBuilder builder;
        private List<string> headers = new List<string>();
        #endregion

        #region Methods
        public void WriteStartElement(string text)
        {
            headers.Add(text);

            builder.Append("<");
            builder.Append(text);
            builder.Append(">");
        }

        public void WriteEndElement()
        {
            int index = headers.Count - 1;
            string text = headers[index];
            headers.RemoveAt(index);

            builder.Append("</");
            builder.Append(text);
            builder.Append(">");
        }

        public void WriteStartElementAndContent(string name, string content)
        {
            builder.Append("<" + name + ">");
            builder.Append(content);
            builder.Append("</" + name + ">");
        }

        public void WriteStartElementAndContent(string name, int content)
        {
            builder.Append("<" + name + ">");
            builder.Append(content);
            builder.Append("</" + name + ">");
        }

        public void WriteStartElementAndContent(string name, bool content)
        {
            builder.Append("<" + name + ">");
            builder.Append(content ? "1" : "0");
            builder.Append("</" + name + ">");
        }

        public void WriteStartElementAndContent(string name, object content)
        {
            builder.Append("<" + name + ">");
            builder.Append(content);
            builder.Append("</" + name + ">");
        }

        public void WriteStartElementAndContent(string name, float content)
        {
            builder.Append("<" + name + ">");
            builder.Append(content);
            builder.Append("</" + name + ">");
        }

        public void WriteStartElementAndContent(string name, double content)
        {
            builder.Append("<" + name + ">");
            builder.Append(content);
            builder.Append("</" + name + ">");
        }

        public void WriteStartElementAndEmptyContent(string name)
        {
            builder.Append("<" + name + ">");
            builder.Append("</" + name + ">");
        }

        public void WriteStartElementAndSimpleEndElement(string name)
        {
            builder.Append("<" + name + "/>");
        }

        public void WriteSimpleEndElement()
        {
            int index = headers.Count - 1;
            headers.RemoveAt(index);

            builder.Insert(builder.Length - 1, "/");
        }

        public void WriteString(string value)
        {
            builder.Append(value);
        }

        public void WriteInt(int value)
        {
            builder.Append(value);
        }

        #region WriteAttributeString
        public void WriteSimpleAttribute(string attr, int value)
        {
            string str = " " + attr + "=\"" + value + "\"";
            builder.Insert(builder.Length - 1, str);
        }
        #endregion

        public override string ToString()
        {
            return (this.IsEncodeString) ? StiSLEncodingHelper.EncodeString(builder.ToString()) : builder.ToString();
        }
        #endregion

        public StiXmlWriter()
        {
            builder = new StringBuilder();
        }
    }
}
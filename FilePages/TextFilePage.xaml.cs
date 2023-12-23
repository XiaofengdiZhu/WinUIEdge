using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace Edge
{
    public sealed partial class TextFilePage : Page
    {
        public string file;

        public List<EncodingInfo> encodeList;

        public string DefaultFontFamily = "Consolas";

        public int DefaultFontSize = 14;

        public TextFilePage(string filepath, string fileType)
        {
            this.InitializeComponent();

            // �����ļ���Ϣ
            file = filepath;

            string DefaultEncoding = "utf-8";
            string content = GetFileText(DefaultEncoding);

            // ���ر����б�
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            encodeList = [.. Encoding.GetEncodings()];
            encodeBox.ItemsSource = encodeList;

            // ��ʼ��UI ����
            fileNameBlock.Text = file;
            EOFType.Text = GetEOF();
            textType.Text = fileType;

            // ���ñ༭���ı�
            editor.Text = content;
            textInfo.Text = $"�� {content.Length} ���ַ�";

            editor.FontFamily = new FontFamily(DefaultFontFamily);
            editor.FontSize = DefaultFontSize;

            encodeBox.SelectedIndex = encodeList.FindIndex(x => x.Name == DefaultEncoding);

        }

        public string GetFileText(string encoding)
        {
            using StreamReader reader = new(file, Encoding.GetEncoding(encoding));
            return reader.ReadToEnd();
        }

        private void FontFamilyChanged(object sender, SelectionChangedEventArgs e)
        {
            editor.FontFamily = new FontFamily((string)(sender as ComboBox).SelectedItem);
        }

        private void FontSizeChanged(object sender, SelectionChangedEventArgs e)
        {
            editor.FontSize = (int)(sender as ComboBox).SelectedItem;
        }

        public string GetEOF()
        {
            using StreamReader reader = File.OpenText(file);

            while (reader.Peek() >= 0)
            {
                char c = (char)reader.Read();

                if (c == '\r')
                {
                    c = (char)reader.Read();

                    if (c == '\n') return "CRLF";
                    else return "CR";
                }
                else if (c == '\n') return "LF";
            }
            return "UnKnown";
        }

        private void EncodeTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            string content = GetFileText(encodeList[(sender as ComboBox).SelectedIndex].Name);
            editor.Text = content;
        }
    }
}
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HACCPExtender.Controllers.Common
{
    public class CsvParser
    {
        /// <summary>
        /// エラーメッセージリスト
        /// </summary>
        public List<string> ErrorMessageList = new List<string>();

        /// <summary>
        /// CSV読込み結果のリスト
        /// </summary>
        public IEnumerable<String[]> csvList = new List<String[]>();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="inputStream">CSV ファイルの Stream</param>
        public CsvParser(Stream inputStream)
        {
            IEnumerable<string[]> csvLines = ReadCsv(inputStream);
            this.csvList = new List<String[]>(csvLines);
        }
        /// <summary>
        /// CSV の Stream を読み込み、string 配列の列挙子を返却
        /// </summary>
        /// <param name="stream">CSV ファイルの Stream</param>
        /// <returns>各項目を string 配列の要素とし、1 行を 1 つの要素とした列挙子</returns>
        private IEnumerable<string[]> ReadCsv(Stream stream)
        {
            using (TextFieldParser parser = new TextFieldParser(stream, Encoding.GetEncoding("Shift_JIS")))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(new[] { "," });
                parser.HasFieldsEnclosedInQuotes = true;

                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    yield return fields;
                }
            }
        }
    }
}
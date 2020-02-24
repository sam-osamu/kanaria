using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Kanaria
{
    public class UcsString
    {
        private readonly string _target;
        private readonly List<RequestParameter> _convertTypes;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="target">処理対象文字列</param>
        private UcsString(string target)
        {
            _target = target;
            _convertTypes = new List<RequestParameter>();
        }

        /// <summary>
        /// このクラスのインスタンスを作成します。
        /// </summary>
        /// <param name="target">処理対象文字列</param>
        /// <returns>インスタンス</returns>
        public static UcsString From(string target)
        {
            return new UcsString(target);
        }

        /// <summary>
        /// このクラスのインスタンスを作成します。
        /// </summary>
        /// <param name="target">処理対象文字列</param>
        /// <returns>インスタンス</returns>
        public static UcsString From(IEnumerable<char> target)
        {
            return new UcsString(new string(target.ToArray()));
        }

        /// <summary>
        /// 文字列を大文字に変換するように設定します。
        /// </summary>
        /// <returns>このインスタンス。ToString()の呼び出しで変換処理を行います。</returns>
        public UcsString UpperCase()
        {
            _convertTypes.Add(new RequestParameter(RequestType.UpperCase));
            return this;
        }

        /// <summary>
        /// 文字列を小文字に変換するように設定します。
        /// </summary>
        /// <returns>このインスタンス。ToString()の呼び出しで変換処理を行います。</returns>
        public UcsString LowerCase()
        {
            _convertTypes.Add(new RequestParameter(RequestType.LowerCase));
            return this;
        }

        /// <summary>
        /// 文字列をひらがなに変換するように設定します。
        /// </summary>
        /// <returns>このインスタンス。ToString()の呼び出しで変換処理を行います。</returns>
        public UcsString Katakana()
        {
            _convertTypes.Add(new RequestParameter(RequestType.Katakana));
            return this;
        }

        /// <summary>
        /// 文字列を全角カタカナに変換するように設定します。
        /// </summary>
        /// <returns>このインスタンス。ToString()の呼び出しで変換処理を行います。</returns>
        public UcsString Hiragana()
        {
            _convertTypes.Add(new RequestParameter(RequestType.Hiragana));
            return this;
        }

        /// <summary>
        /// 文字列を全角に変換するように設定します。
        /// </summary>
        /// <returns>このインスタンス。ToString()の呼び出しで変換処理を行います。</returns>
        public UcsString Wide(ConvertTarget target = ConvertTarget.All)
        {
            _convertTypes.Add(new RequestParameter(RequestType.Wide, target));
            return this;
        }

        /// <summary>
        /// 文字列を半角に変換するように設定します。
        /// </summary>
        /// <returns>このインスタンス。ToString()の呼び出しで変換処理を行います。</returns>
        public UcsString Narrow(ConvertTarget target = ConvertTarget.All)
        {
            _convertTypes.Add(new RequestParameter(RequestType.Narrow, target));
            return this;
        }

        /// <summary>
        /// 文字列を変換し、stringとして返却します。
        /// このメソッドの呼出前にUpperCase()、LowerCase()、Hiragana()、Katakana()、Wide()、Narrow()等を呼び出し、
        /// 変換先の設定を行う必要があります。
        /// </summary>
        /// <returns>変換後文字列</returns>
        public override string ToString()
        {
            var tmpBuffer = _target;

            _convertTypes.ForEach(param =>
            {
                var type = param.Type;
                var target = param.ConvertTarget;

                // 半角文字の場合、濁音等で文字数が2文字に増えるケースもあるので2倍長さを確保しておく
                var resultBufferSize = (type == RequestType.Narrow) ? tmpBuffer.Length * 2 : tmpBuffer.Length;
                // 終端文字考慮で1つ分長くする。
                var sb = new StringBuilder(resultBufferSize + 1);

                switch (type)
                {
                    case RequestType.UpperCase:
                        ToUpperCase(tmpBuffer, (uint) tmpBuffer.Length, sb, (uint) sb.Capacity);
                        break;
                    case RequestType.LowerCase:
                        ToLowerCase(tmpBuffer, (uint) tmpBuffer.Length, sb, (uint) sb.Capacity);
                        break;
                    case RequestType.Hiragana:
                        ToHiragana(tmpBuffer, (uint) tmpBuffer.Length, sb, (uint) sb.Capacity);
                        break;
                    case RequestType.Katakana:
                        ToKatakana(tmpBuffer, (uint) tmpBuffer.Length, sb, (uint) sb.Capacity);
                        break;
                    case RequestType.Wide:
                        ToWide(tmpBuffer, (uint) tmpBuffer.Length, sb, (uint) sb.Capacity, (uint) target);
                        break;
                    case RequestType.Narrow:
                        ToNarrow(tmpBuffer, (uint) tmpBuffer.Length, sb, (uint) sb.Capacity, (uint) target);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }

                tmpBuffer = sb.ToString();
            });

            return tmpBuffer;
        }

        [DllImport("kanaria.dll", EntryPoint = "to_upper_case_for_utf16",
            CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern uint ToUpperCase(string target, uint targetSize, StringBuilder result, uint resultSize);

        [DllImport("kanaria.dll", EntryPoint = "to_lower_case_for_utf16",
            CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern uint ToLowerCase(string target, uint targetSize, StringBuilder result, uint resultSize);

        [DllImport("kanaria.dll", EntryPoint = "to_hiragana_for_utf16",
            CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern uint ToHiragana(string target, uint targetSize, StringBuilder result, uint resultSize);

        [DllImport("kanaria.dll", EntryPoint = "to_katakana_for_utf16",
            CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern uint ToKatakana(string target, uint targetSize, StringBuilder result, uint resultSize);

        [DllImport("kanaria.dll", EntryPoint = "to_wide_for_utf16",
            CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern uint ToWide(string target, uint targetSize, StringBuilder result, uint resultSize, uint convertTarget);

        [DllImport("kanaria.dll", EntryPoint = "to_narrow_for_utf16",
            CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern uint ToNarrow(string target, uint targetSize, StringBuilder result, uint resultSize, uint convertTarget);

        private enum RequestType
        {
            UpperCase,
            LowerCase,
            Katakana,
            Hiragana,
            Wide,
            Narrow
        }
        
        private class RequestParameter
        {
            public RequestType Type { get; set; }
            public ConvertTarget ConvertTarget { get; set; }
            public RequestParameter(RequestType type, ConvertTarget convertTarget)
            {
                Type = type;
                ConvertTarget = convertTarget;
            }
            
            public RequestParameter(RequestType type)
            {
                Type = type;
                ConvertTarget = ConvertTarget.All;
            }
        }
    }
}
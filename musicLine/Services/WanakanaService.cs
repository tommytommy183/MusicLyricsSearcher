using MeCab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WanaKanaNet;

namespace musicLine.Services
{
    public class WanakanaService
    {
        public MeCabTagger _tagger = MeCabTagger.Create();
        public string ToHiragana(string input)
        {
            var result = WanaKana.ToHiragana(input);
            return result;
        }

        public string ToKatakana(string input)
        {
            var result = WanaKana.ToKatakana(input);
            return result;
        }
        public string ToRomaji(string input)
        {
            var result = WanaKana.ToRomaji(input);
            return result;
        }
    }
}

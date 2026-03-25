using MeCab;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

public class FuriganaLabel : Control
{
    private List<List<(string baseText, string furigana, float x)>> lines = new();
    private Font baseFont = new Font("MS Gothic", 18);
    private Font smallFont = new Font("MS Gothic", 10);

    private MeCabTagger _tagger;
    private Func<string, string> _toHiragana;

    public FuriganaLabel()
    {
        DoubleBuffered = true;
    }

    public void Init(MeCabTagger tagger, Func<string, string> toHiragana)
    {
        _tagger = tagger;
        _toHiragana = toHiragana;
    }

    public void SetText(string input)
    {
        BuildLayout(input);
        Invalidate();
    }

    private void BuildLayout(string input)
    {
        lines.Clear();
        if (_tagger == null) return;

        float maxWidth = this.Width - Padding.Left - Padding.Right;
        List<(string baseText, string furigana, float x)> currentLine = new();
        float x = 0;

        foreach (var node in _tagger.ParseToNodes(input))
        {
            if (node.Stat == MeCabNodeStat.Bos || node.Stat == MeCabNodeStat.Eos)
                continue;

            var surface = node.Surface;
            var features = node.Feature.Split(',');
            string reading = features.Length > 7 ? features[7] : "";

            bool hasKanji = surface.Any(c => c >= 0x4E00 && c <= 0x9FFF);
            bool hasKata = surface.Any(c => c >= 0x30A0 && c <= 0x30FF);

            string hira = "";
            if ((hasKanji || hasKata) && !string.IsNullOrEmpty(reading) && reading != "*")
                hira = _toHiragana(reading);

            float width = TextRenderer.MeasureText(surface, baseFont).Width;

            // 超過寬度就換行
            if (x + width > maxWidth)
            {
                lines.Add(currentLine);
                currentLine = new();
                x = 0;
            }

            currentLine.Add((surface, hira, x));
            x += width;
        }

        if (currentLine.Count > 0)
            lines.Add(currentLine);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;

        float y = 10;

        foreach (var line in lines)
        {
            // 找到這行最高的平假名高度
            float hiraHeight = smallFont.Height;
            float baseHeight = baseFont.Height;

            foreach (var item in line)
            {
                if (!string.IsNullOrEmpty(item.furigana))
                    g.DrawString(item.furigana, smallFont, Brushes.Black, item.x + Padding.Left, y);
            }

            // 畫原文
            foreach (var item in line)
            {
                g.DrawString(item.baseText, baseFont, Brushes.Black, item.x + Padding.Left, y + hiraHeight);
            }

            // 換到下一行
            y += hiraHeight + baseHeight + 2; // 2像素間距
        }
    }
}
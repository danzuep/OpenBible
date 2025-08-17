namespace Unihan.Models
{
    //[Flags]
    public enum UnihanField
    {
        Unknown = 0,
        kBopomofo,
        kCantonese,
        kDefinition,
        kFanqie,
        kHangul,
        kHanyuPinlu,
        kHanyuPinyin,
        kJapanese,
        kJapaneseKun,
        kJapaneseOn,
        kKorean,
        kMandarin,
        kSMSZD2003Readings,
        kTang,
        kTGHZ2013,
        kVietnamese,
        kXHC1983,
        kZhuang
    }

    /// See: https://www.unicode.org/cgi-bin/GetUnihanData.pl?codepoint=7684
    /// See also: kZhuang, and kBopomofo (Zhuyin Fuhao)
    //U+7684	kCantonese	dik1
    //U+7684	kDefinition	possessive, adjectival suffix
    //U+7684	kFanqie	都歷
    //U+7684	kHangul	적:0E
    //U+7684	kHanyuPinlu	de(75596) dì(157) dí(84)
    //U+7684	kHanyuPinyin	42644.160:dì,dí,de
    //U+7684	kJapanese	テキ チャク キョウ ギョウ まと あきらか
    //U+7684	kJapaneseKun	MATO AKIRAKA
    //U+7684	kJapaneseOn	TEKI
    //U+7684	kKorean	CEK
    //U+7684	kMandarin	de
    //U+7684	kSMSZD2003Readings	dì粵dik1 dí粵dik1 de粵dik1
    //U+7684	kTGHZ2013	069.080:de 070.170:dī 071.080:dí 072.100:dì
    //U+7684	kTang	dek
    //U+7684	kVietnamese	đích
    //U+7684	kXHC1983	0224.040:de 0231.060:dí 0239.060:dì
}
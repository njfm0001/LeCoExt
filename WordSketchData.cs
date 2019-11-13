using System.Collections.Generic;

namespace CWordSketch_Extractor
{
    public class JsonWordSketch
    {
        public Gramrel[] Gramrels { get; set; }
        public Lpos_Dict lpos_dict { get; set; }
        public Alt_Lposes[] alt_lposes { get; set; }
        public string lemma_conc_link { get; set; }
        public string restriction { get; set; }
        public string seppage { get; set; }
        public object[] Histograms { get; set; }
        public WordSketchAPIRequest Request { get; set; }
        public string lpos { get; set; }
        public string api_version { get; set; }
        public string corp_full_name { get; set; }
        public string lemma { get; set; }
        public Grnoword[] Grnowords { get; set; }
        public int minfreq { get; set; }
        public string gramrels { get; set; }
        public string filterseek { get; set; }
        public int freq { get; set; }
        public float relfreq { get; set; }
        public string[][] Bimmenu { get; set; }
        public string filterwords { get; set; }
        public bool combined { get; set; }
    }
    public class WordSketchAPIRequest
    {
        public string corpname { get; set; }
        public string format { get; set; }
        public string lpos { get; set; }
        public int sort_gramrels { get; set; }
        public string lemma { get; set; }
        public float minscore { get; set; }
        public int maxItems { get; set; }
        public int expand_seppage { get; set; }
        public string username { get; set; }
        public string api_key { get; set; }
    }

    public class Lpos_Dict
    {
        public string adjective { get; set; }
        public string verb { get; set; }
        public string noun { get; set; }
        public string pronoun { get; set; }
        public string adverb { get; set; }
    }

    public class Gramrel
    {
        public object count { get; set; }
        public string name { get; set; }
        public string conclink { get; set; }
        public object score { get; set; }
        public Word[] Words { get; set; }
        public string headword { get; set; }
        public string seek { get; set; }
        public int colbreak { get; set; }
        public int spid { get; set; }
        public int pagebreak { get; set; }
    }

    public class Word
    {
        public string cm { get; set; }
        public string filterseek { get; set; }
        public string lempos { get; set; }
        public string seek { get; set; }
        public int id { get; set; }
        public string filterwords { get; set; }
        public int count { get; set; }
        public string word { get; set; }
        public int cmf { get; set; }
        public string conclink { get; set; }
        public float score { get; set; }
        public int colbreak { get; set; }
        public string name { get; set; }
        public Word1[] Words { get; set; }
        public string headword { get; set; }
        public int pagebreak { get; set; }
    }
    public class Word1
    {
        public string cm { get; set; }
        public string filterseek { get; set; }
        public string lempos { get; set; }
        public string seek { get; set; }
        public int id { get; set; }
        public string filterwords { get; set; }
        public int count { get; set; }
        public string word { get; set; }
        public int cmf { get; set; }
        public string conclink { get; set; }
        public float score { get; set; }
    }

    public class Alt_Lposes
    {
        public string lemma { get; set; }
        public int frq { get; set; }
        public string pos { get; set; }
        public string lempos { get; set; }
    }

    public class Grnoword
    {
        public int count { get; set; }
        public string name { get; set; }
        public string conclink { get; set; }
        public float score { get; set; }
        public object[] Words { get; set; }
        public string headword { get; set; }
        public string seek { get; set; }
    }

    public class CorpusSearch
    {
        public string corpusName { get; set; }
        public List<Lemma> corpusWS { get; set; }
        public CorpusSearch(string corpusName, List<Lemma> corpusWS)
        {
            this.corpusName = corpusName;
            this.corpusWS = corpusWS;
        }
    }
    public class Lemma
    {
        public string lemma { get; set; }
        public List<GramRels> gramrels { get; set; }
        public Lemma(string lemma, List<GramRels> gramrels)
        {
            this.lemma = lemma;
            this.gramrels = gramrels;
        }
    }

    public class GramRels
    {
        public string gramRelName { get; set; }
        public List<CollocateInfo> CollocateInfo { get; set; }
        public GramRels(string gramRelName, List<CollocateInfo> CollocateInfo)
        {
            this.gramRelName = gramRelName;
            this.CollocateInfo = CollocateInfo;
        }
    }

    public class CollocateInfo
    {
        public string lemma { get; set; }
        public string collocate { get; set; }
        public double score { get; set; }
    }
}

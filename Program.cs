using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using EasyHttp.Http;
using Newtonsoft.Json;

namespace CWordSketch_Extractor
{
    static class LECoExt
    {
        //Further documentation details about the SketchEngine API can be found on https://www.sketchengine.eu/documentation/api-documentation

        static readonly string url = "https://api.sketchengine.co.uk/bonito/run.cgi";        
        static Lemma Lemma { get; set; }
        static CorpusSearch Crps { get; set; }
        static List<CorpusSearch> CorpusSearchList { get; set; }

        static List<CollocateInfo> SubjectList { get; set; }
        static List<CollocateInfo> ObjectList { get; set; }
        static List<CollocateInfo> ObliqueObjectList { get; set; }
        //Specific semantic roles or frames for STEALING VERBS. These are subject to change for other verbs. Plus, they might not coincide with syntactic constituents (i.e. gramrels) in a one-to-one relationship
        static List<CollocateInfo> PerpetratorList { get; set; }
        static List<CollocateInfo> GoodsList { get; set; }
        static List<CollocateInfo> SourceList { get; set; }

        static List<CorpusSearch> CollocateExtraction(string[] verbs, List<string> prepObliqueObject, string[] corpora, string user, string apiKey)
        {
            WordSketchAPIRequest wordsketchData = new WordSketchAPIRequest() //set the API connection
            {
                corpname = "",
                format = "json",
                lpos = "-v",
                sort_gramrels = 1,
                minscore = 0.0f,
                maxItems = 200,
                expand_seppage = 1,
                username = user,
                api_key = apiKey,
            };
            List<CorpusSearch> corpusSearch = new List<CorpusSearch>();
            foreach (string corpus in corpora)
            {
                List<Lemma> lemmasList = new List<Lemma>();                
                wordsketchData.corpname = corpus;
                foreach (string verb in verbs)
                {
                    wordsketchData.lemma = verb;
                    HttpClient http = new HttpClient(); //set the HTTP request
                    http.Request.Accept = HttpContentTypes.ApplicationJson;
                    HttpResponse result = http.Get(url + "/wsketch", wordsketchData);
                    Console.WriteLine(Environment.NewLine);
                    Console.WriteLine("Corpus: {0}", corpus);
                    Console.WriteLine("WordSketch for Lemma: {0}", verb);
                    Console.WriteLine("Retrieving lexical collocate lists...");
                    string test = result.RawText;
                    JsonWordSketch jobj = JsonConvert.DeserializeObject<JsonWordSketch>(test);
                    Gramrel[] gramRels = jobj.Gramrels;
                    List<GramRels> gramrelList = new List<GramRels>();
                    if (gramRels != null)
                    {
                        foreach (Gramrel gramrel in gramRels)
                        {
                            if (gramrel.name.Contains("subject") || gramrel.name.Contains("object") || prepObliqueObject.Contains(gramrel.name))
                            if (gramrel.name == "subject" || gramrel.name == "object" || gramrel.name == @"subjects of ""%w""" || gramrel.name == @"objects of ""%w""" || prepObliqueObject.Contains(gramrel.name))
                            {
                                Console.WriteLine(Environment.NewLine);
                                Console.WriteLine("Gramrel: {0}", gramrel.name.ToUpper());
                                Word[] collocates = gramrel.Words;
                                List<CollocateInfo> collocateList = new List<CollocateInfo>();
                                foreach (Word collocate in collocates)
                                {
                                    CollocateInfo col = new CollocateInfo
                                    {
                                        collocate = collocate.word.ToLower(),
                                        score = collocate.score,
                                        lemma = verb
                                    };
                                    collocateList.Add(col);
                                    Console.WriteLine("Word: {0} {1}", collocate.word, collocate.score.ToString());
                                }
                                GramRels grmrl = new GramRels(gramrel.name, collocateList);
                                gramrelList.Add(grmrl);
                            }
                        }
                    }
                    Lemma = new Lemma(verb, gramrelList);
                    lemmasList.Add(Lemma);
                }
                Crps = new CorpusSearch(corpus, lemmasList);
                corpusSearch.Add(Crps);
                Thread.Sleep(3000);
            }
            return corpusSearch;
        }

        static void GetCollocatesVerbs(List<CorpusSearch> corpusSearchList, bool isStealingVerb, List<string> prepObliqueObject)
        {
            PerpetratorList = new List<CollocateInfo>();
            GoodsList = new List<CollocateInfo>();
            SourceList = new List<CollocateInfo>();

            SubjectList = new List<CollocateInfo>();
            ObjectList = new List<CollocateInfo>();
            ObliqueObjectList = new List<CollocateInfo>();

            foreach (CorpusSearch corpus in corpusSearchList)
            {
                foreach (Lemma lm in corpus.corpusWS)
                {
                    foreach (GramRels grammrel in lm.gramrels)
                    {
                        foreach (CollocateInfo clclt in grammrel.CollocateInfo)
                        {
                            if (!Regex.IsMatch(clclt.collocate, @".*\d+")) //with this regex rule, we discard false positives of random combinations of numbers and letters appearing as collocates
                            {   
                               if (isStealingVerb)
                               {
                                    if (grammrel.gramRelName.Contains("subject")) { PerpetratorList.Add(clclt); }
                                    else if (grammrel.gramRelName.Contains("object"))
                                    {
                                        if (lm.lemma == "rob" || lm.lemma == "hold up" || lm.lemma == "stick up" || lm.lemma == "plunder" || lm.lemma == "despoil" || lm.lemma == "pillage" || lm.lemma == "loot")
                                        {
                                            SourceList.Add(clclt);
                                            if (lm.lemma == "plunder" || lm.lemma == "pillage" || lm.lemma == "loot") { GoodsList.Add(clclt); }
                                        }
                                        else { GoodsList.Add(clclt); }
                                    }
                                    else if (grammrel.gramRelName.Contains("from"))
                                    {
                                        if (lm.lemma != "rob" && lm.lemma != "hold up" && lm.lemma != "stick up" && lm.lemma != "plunder" && lm.lemma != "despoil" && lm.lemma != "pillage" && lm.lemma != "loot")
                                        {
                                            SourceList.Add(clclt);
                                        }
                                    }
                                    else if (grammrel.gramRelName == "pp_of -p" || grammrel.gramRelName == @"""%w"" of ...")
                                    {
                                        if (lm.lemma == "rob" || lm.lemma == "hold up" || lm.lemma == "stick up" || lm.lemma == "despoil") { GoodsList.Add(clclt); }

                                    }
                               }

                               else
                               {
                                    if (grammrel.gramRelName.Contains("subject")) { SubjectList.Add(clclt); }
                                    else if (grammrel.gramRelName.Contains("object")) { ObjectList.Add(clclt); }
                                    else if (prepObliqueObject.Any(t => grammrel.gramRelName.Contains(t))) { ObliqueObjectList.Add(clclt); }
                               }
                            }
                        }
                    }
                }
            }
        }
     
        //With this method, we calculate the Collocate Frequency Score (CFS). Details about the calculations performed can be found on Fernández-Martínez & Felices-Lago (forthcoming)     
        static List<CollocateInfo> CalculateCFS(List<CollocateInfo> collocateList, string[] verbs, string[] corpora)
        {
            collocateList = collocateList.OrderBy(p => p.collocate).ThenBy(r => r.lemma).ThenBy(q => q.score).ToList();
            for (int i = 0; i < collocateList.Count; ++i)
            {
                double CFS = collocateList[i].score;
                double countofinstances = 0;
                double numberofverbs = 1;
                double weightfactor = 0;
                while (i + 1 < collocateList.Count)
                {
                    if (collocateList[i].collocate == collocateList[i + 1].collocate)
                    {
                        CFS += collocateList[i + 1].score;                        
                        if (collocateList[i].lemma != collocateList[i + 1].lemma) { numberofverbs++; countofinstances++; }
                        collocateList.RemoveAt(i);
                        i--;
                    }                    
                    else { break; }
                    i++;
                }
                weightfactor = numberofverbs / verbs.Count();
                CFS = CFS / (corpora.Count() + countofinstances);
                CFS = CFS * weightfactor;
                collocateList[i].score = CFS;
            }
            collocateList = collocateList.OrderByDescending(p => p.score).ToList();
            return collocateList;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("***************LExical Collocate Extractor***************");
            Console.WriteLine("developed by Nicolás José Fernández Martínez");
            Console.WriteLine();
            while(true)
            {
                Console.WriteLine("Enter your WordSketch user and API key (e.g. user XXXXXXXXXXXXXX). If you do not have an account, you can register for a free 30-day trial on https://www.sketchengine.eu/");
                string userResponseLogin = Console.ReadLine();
                if (userResponseLogin == "") { continue; }
                string[] userCredentials = userResponseLogin.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                   .Where(x => !string.IsNullOrWhiteSpace(x))
                   .Select(s => s.Trim()).ToArray();

                Console.WriteLine();
                Console.WriteLine("Write the verb(s) for the lexical collocate extraction process separated by commas: (e.g. verbs of stealing: steal, rob, hold up, stick up, thieve, embezzle, purloin, pilfer, pinch, filch, lift, nick, swipe, rustle, clean out, make off with, plunder, pillage, despoil, loot)");
                string userResponseVerbs = Console.ReadLine();
                if (userResponseVerbs == "") { continue; }
                string[] verbs = userResponseVerbs.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(s => s.Trim()).ToArray();

                bool isStealingVerb = false;
                string[] stealingVerbs = new string[] { "steal", "rob", "plunder", "hold up", "stick up", "thieve", "embezzle", "purloin", "shoplift", "pilfer", "pinch", "filch", "lift", "nick", "swipe", "rustle", "clean out", "make off with", "plunder", "pillage", "despoil", "loot" };
                if (stealingVerbs.Any(t => verbs.Contains(t))) { isStealingVerb = true; }

                Console.WriteLine();
                Console.WriteLine("Do any of them contain an oblique object? Press ENTER if they do not. If they do, indicate the preposition that introduces oblique objects: to, from, of (e.g. of, from)");
                string userResponseObliqueObject = Console.ReadLine();
                List<string> prepositionObliqueObject = userResponseObliqueObject.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(s => s.Trim()).ToList();
                for (int i = 0; i < prepositionObliqueObject.Count; i++)
                {
                    if (prepositionObliqueObject[i] == "of") { prepositionObliqueObject[i] = "pp_of -p"; prepositionObliqueObject.Add(@"""%w"" of ..."); }
                    else if (prepositionObliqueObject[i] == "from") { prepositionObliqueObject[i] = "pp_from-p"; prepositionObliqueObject.Add(@"""%w"" from ..."); }
                    else if (prepositionObliqueObject[i] == "to") { prepositionObliqueObject[i] = "pp_to -p"; prepositionObliqueObject.Add(@"""%w"" to ..."); }
                }
                Console.WriteLine("Write the corpus or corpora for the lexical collocate extraction process separated by commas. Current options are: preloaded/bnc2, preloaded/ententen13_tt2_1, preloaded/ententen15_tt21, eng_jsi_newsfeed_1");
                string userResponseCorpora = Console.ReadLine();
                string[] corpora = userResponseCorpora.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(s => s.Trim()).ToArray();
                Console.WriteLine();
                Console.WriteLine("The gramrels consulted are: Subjects, Objects and Prepositional Objects (if any, introduced by from-, to- and/or of-phrases).");

                //Perform collocate extraction from WordNet                
                CorpusSearchList = CollocateExtraction(verbs, prepositionObliqueObject, corpora, userCredentials[0], userCredentials[1]); //get the collocates and their scores for each of the corpora for each of verbs for each of the main syntactic constituents (i.e. gramrels)
                GetCollocatesVerbs(CorpusSearchList, isStealingVerb, prepositionObliqueObject); //Get collocates for verbs
                if (isStealingVerb)
                {
                    PerpetratorList = CalculateCFS(PerpetratorList, verbs, corpora);
                    GoodsList = CalculateCFS(GoodsList, verbs, corpora);
                    SourceList = CalculateCFS(SourceList, verbs, corpora);
                }
                else
                {
                    SubjectList = CalculateCFS(SubjectList, verbs, corpora);
                    ObjectList = CalculateCFS(ObjectList, verbs, corpora);
                    ObliqueObjectList = CalculateCFS(ObliqueObjectList, verbs, corpora);
                }

                string subjects = "LIST OF SUBJECT COLLOCATES";
                string objects = "LIST OF OBJECT COLLOCATES";
                string obliqueobjects = "LIST OF OBLIQUE OBJECTS";
                if (verbs.Contains("steal"))
                {
                    subjects = "LIST OF PERPETRATORS FOR THE CONCEPTUAL SCENARIO OF THEFT";
                    objects = "LIST OF GOODS FOR THE CONCEPTUAL SCENARIO OF THEFT";
                    obliqueobjects = "LIST OF SOURCE FOR THE CONCEPTUAL SCENARIO OF THEFT";
                }

                StringBuilder sb = new StringBuilder();
                Console.WriteLine();
                Console.WriteLine(subjects);
                sb.AppendLine(subjects);
                if (isStealingVerb)
                {
                    foreach (CollocateInfo collocate in PerpetratorList)
                    {
                        Console.WriteLine("Word: {0} : {1}", collocate.collocate, collocate.score);
                        sb.AppendLine("Word: " + collocate.collocate + " : " + collocate.score);
                    }
                }
                else
                {
                    foreach (CollocateInfo collocate in SubjectList)
                    {
                        Console.WriteLine("Word: {0} : {1}", collocate.collocate, collocate.score);
                        sb.AppendLine("Word: " + collocate.collocate + " : " + collocate.score);
                    }
                }

                Thread.Sleep(3000);
                sb.AppendLine();
                Console.WriteLine();
                Console.WriteLine(objects);
                sb.AppendLine(objects);
                if (isStealingVerb)
                {
                    foreach (CollocateInfo collocate in GoodsList)
                    {
                        Console.WriteLine("Word: {0} : {1}", collocate.collocate, collocate.score);
                        sb.AppendLine("Word: " + collocate.collocate + " : " + collocate.score);
                    }
                }
                else
                {
                    foreach (CollocateInfo collocate in ObjectList)
                    {
                        Console.WriteLine("Word: {0} : {1}", collocate.collocate, collocate.score);
                        sb.AppendLine("Word: " + collocate.collocate + " : " + collocate.score);
                    }
                }

                Thread.Sleep(3000);
                sb.AppendLine();
                Console.WriteLine();
                Console.WriteLine(obliqueobjects);
                sb.AppendLine(obliqueobjects);
                if (isStealingVerb)
                {
                    foreach (CollocateInfo collocate in SourceList)
                    {
                        Console.WriteLine("Word: {0} : {1}", collocate.collocate, collocate.score);
                        sb.AppendLine("Word: " + collocate.collocate + " : " + collocate.score);
                    }
                }
                else
                {
                    foreach (CollocateInfo collocate in ObliqueObjectList)
                    {
                        Console.WriteLine("Word: {0} : {1}", collocate.collocate, collocate.score);
                        sb.AppendLine("Word: " + collocate.collocate + " : " + collocate.score);
                    }
                }

                Thread.Sleep(3000);
                Console.WriteLine();
                Console.WriteLine("Saving output to output.txt...");
                string output = sb.ToString();
                string fileName = "output.txt";
                string path = Path.Combine(Environment.CurrentDirectory, fileName);
                File.WriteAllText(path, output);
                Console.WriteLine("Press ESC to exit or any other key to perform another search.");
                ConsoleKeyInfo key = Console.ReadKey();
                if (key.Key == ConsoleKey.Escape)
                {
                    Environment.Exit(0);
                }
                Console.WriteLine();
            }           
        }
    }
}

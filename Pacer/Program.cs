using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Threading;

using opennlpinterface;

namespace Pacer
{
    public class NLPTool
    {
        NLPToolBox nlp_models = null;
        public NLPTool(string SSModel, string TKModel, string POSModel,
            string CKModel, string PersonModel, string OrgModel, string LocModel,
            string DateModel, string MoneyModel, string PercentageModel, string TimeModel,
            string ParseModel, string MaltParseModel)
        {
            nlp_models = new NLPToolBox(SSModel, TKModel, POSModel, CKModel, PersonModel,
                OrgModel, LocModel, DateModel, MoneyModel, PercentageModel, TimeModel,
                ParseModel, MaltParseModel);
        }
        public List<string> Tokenize(string sentence)
        {
            List<string> ret = new List<string>();
            opennlp.tools.util.Span[] tkpos =
                nlp_models.sentenceTokenizer.tokenizePos(sentence);
            for (int i = 0; i < tkpos.Length; i++)
            {
                ret.Add(sentence.Substring(tkpos[i].getStart(), tkpos[i].length()));
            }
            return ret;
        }
        public List<string> SentenceSegment(string raw_corpus)
        {
            List<string> ret = new List<string>();
            opennlp.tools.util.Span[] ss =
                nlp_models.sentenceDetector.sentPosDetect(raw_corpus);
            for (int i = 0; i < ss.Length; i++)
            {
                ret.Add(raw_corpus.Substring(ss[i].getStart(), ss[i].length()));
            }
            return ret;
        }
    }
    class Program
    {
        static string output_folder = @"..\..\..\Data\ProcessedCorpus\Tokenization\";
        static string corpus_filename = @"..\..\..\Corpus\toy_corpus.txt";
        // static string corpus_filename = @"..\..\..\Corpus\WestburyLab.wikicorp.201004.txt";
        static void Main(string[] args)
        {
            // Build the tokenizer
            string toolbox_folder = @"..\..\..\NLPTech\Toolbox\";
            string SSModel = toolbox_folder + "en-sent.bin";
            string TKModel = toolbox_folder + "en-token.bin";
            string POSModel = toolbox_folder + "en-pos-maxent.bin";
            string CKModel = toolbox_folder + "en-chunker.bin";
            string PersonModel = toolbox_folder + "en-ner-person.bin";
            string OrgModel = toolbox_folder + "en-ner-organization.bin";
            string LocModel = toolbox_folder + "en-ner-location.bin";
            string DateModel = toolbox_folder + "en-ner-date.bin";
            string MoneyModel = toolbox_folder + "en-ner-money.bin";
            string PercentageModel = toolbox_folder + "en-ner-percentage.bin";
            string TimeModel = toolbox_folder + "en-ner-time.bin";
            string ParseModel = toolbox_folder + "en-parser-chunking.bin";
            string MaltParseModel = toolbox_folder + "engmalt.linear-1.7.mco";
            NLPTool nlptool = new NLPTool(SSModel, TKModel, POSModel, CKModel,
                PersonModel, OrgModel, LocModel, DateModel, MoneyModel, PercentageModel,
                TimeModel, ParseModel, MaltParseModel);
            // Load the corpus
            string output_filename = output_folder + Path.GetFileName(corpus_filename);
            StreamReader fin = new StreamReader(corpus_filename);
            StreamWriter fout = new StreamWriter(output_filename);
            for (string line = fin.ReadLine(); line != null; line = fin.ReadLine())
            {
                line = line.Replace('"', ' ');
                List<string> sentences = nlptool.SentenceSegment(line);
                foreach (string sentence in sentences)
                {
                    List<string> words = nlptool.Tokenize(sentence);
                    foreach (string word in words)
                    {
                        try
                        {
                            byte[] bArray = Encoding.UTF8.GetBytes(word);
                            foreach (byte bElement in bArray)
                            {
                                fout.Write((char)bElement);
                            }
                            fout.Write(" ");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("{0} Exception caught.", e);
                        }
                    }
                }
                fout.WriteLine();
            }
            fin.Close();
            fout.Close();
        }
    }
}

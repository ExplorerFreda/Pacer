using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Threading;

using opennlpinterface;
using opennlp.tools.lemmatizer;


namespace Pacer
{
    public class NLPTool
    {
        private NLPToolBox nlp_models = null;
		private SimpleLemmatizer lemmatizer = null;
        public NLPTool(string SSModel, string TKModel, string POSModel,
            string CKModel, string PersonModel, string OrgModel, string LocModel,
            string DateModel, string MoneyModel, string PercentageModel, string TimeModel,
			string ParseModel, string MaltParseModel, string LemmatizeDict)
        {
            nlp_models = new NLPToolBox(SSModel, TKModel, POSModel, CKModel, PersonModel,
                OrgModel, LocModel, DateModel, MoneyModel, PercentageModel, TimeModel,
                ParseModel, MaltParseModel);
			java.io.InputStream input_stream = new java.io.FileInputStream(LemmatizeDict);
			lemmatizer = new SimpleLemmatizer(input_stream);
			input_stream.close();
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
		public string POSTagger(string sentence)
		{
			string[] tokens = sentence.Split(' ');
			string[] postags = nlp_models.tagger.tag(tokens);
			string ret = "";
			foreach (string pos in postags)
			{
				ret += pos + " ";
			}
			return ret.Substring(0, ret.Length - 1);
		}
		public string Lemmatization(string sentence, string postag)
		{
			string[] words = sentence.Split(' ');
			string[] postags = postag.Split(' ');
			string ret = "";
			for (int i = 0; i < words.Length; i++)
			{
				string lemma = lemmatizer.lemmatize(words[i], postags[i]);
				ret += lemma + " ";
			}
			return ret.Substring(0, ret.Length - 1);
		}
    }
    class Program
    {
		static NLPTool nlptool = null;
        static string output_folder = @"..\..\..\Data\ProcessedCorpus\Tokenization\";
        static string corpus_filename = @"..\..\..\Corpus\toy_corpus.txt";
        // static string corpus_filename = @"..\..\..\Corpus\WestburyLab.wikicorp.201004.txt";

		static void Tokenization(string corpus_filename, string output_filename)
		{
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
		static void POSTagging(string corpus_filename, string output_filename, string config = "default")
		{
			StreamReader fin = new StreamReader(corpus_filename);
			StreamWriter fout = new StreamWriter(output_filename);
			switch (config) {
				case "TroFi":
					for (string line = fin.ReadLine(); line != null; line = fin.ReadLine())
					{
						if (line == "********************") // seprate tag
						{
							fout.WriteLine(line);
						} 
						else if (line.StartsWith("***")) // keyword tag
						{
							string key_word = line.Substring(3, line.Length - 6);
							fout.WriteLine(line);
						}
						else if (line.StartsWith("*")) // literal cluster || non-literal cluster tag
						{
							fout.WriteLine(line);
						}
						else if (line.Length < 2) // empty line
						{
							fout.WriteLine(line);
						}
						else // data line
						{
							string[] data = line.Split('\t');
							if (data.Length == 3)
							{
								string postag = nlptool.POSTagger(data[2]);
								fout.WriteLine(data[0] + '\t' + data[1] + '\t' + data[2] + '\t' + postag);
								if (postag.Split().Length != data[2].Split().Length) // unexpected case, for debugging
								{
									Console.WriteLine("Error Type I: " + line);
									while (true) { }
								}
							}
							else // unexpect case, for debugging
							{
								Console.WriteLine("Error Type II: " + line);
								while (true) { }
							}
						}
					}
					break;
				default:
					break;
			}
			fin.Close();
			fout.Close();
		}
		static void Lemmatization(string corpus_filename, string output_filename, string config)
		{
			StreamReader fin = new StreamReader(corpus_filename);
			StreamWriter fout = new StreamWriter(output_filename);
			switch (config)
			{
				case "TroFi":
					for (string line = fin.ReadLine(); line != null; line = fin.ReadLine())
					{
						if (line == "********************") // seprate tag
						{
							fout.WriteLine(line);
						}
						else if (line.StartsWith("***")) // keyword tag
						{
							string key_word = line.Substring(3, line.Length - 6);
							fout.WriteLine(line);
						}
						else if (line.StartsWith("*")) // literal cluster || non-literal cluster tag
						{
							fout.WriteLine(line);
						}
						else if (line.Length < 2) // empty line
						{
							fout.WriteLine(line);
						}
						else // data line
						{
							string[] data = line.Split('\t');
							if (data.Length == 4)
							{
								string lemmatization = nlptool.Lemmatization(data[2], data[3]);
								fout.WriteLine(data[0] + '\t' + data[1] + '\t' + data[2] + '\t' + data[3] + '\t' + lemmatization);
								if (lemmatization.Split().Length != data[2].Split().Length) // unexpected case, for debugging
								{
									Console.WriteLine("Error Type I: " + line);
									while (true) { }
								}
							}
							else // unexpect case, for debugging
							{
								Console.WriteLine("Error Type II: " + line);
								while (true) { }
							}
						}
					}
					break;
				default:
					break;
			}
			fin.Close();
			fout.Close();
		}
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
			string LemmatizeDict = toolbox_folder + "en-lemmatizer.dict";
            nlptool = new NLPTool(SSModel, TKModel, POSModel, CKModel,
                PersonModel, OrgModel, LocModel, DateModel, MoneyModel, PercentageModel,
				TimeModel, ParseModel, MaltParseModel, LemmatizeDict);
			/*
			 * This is the toy for tokenization.
			string output_filename = output_folder + Path.GetFileName(corpus_filename);
			Tokenization(corpus_filename, output_filename);
			 */

			/*
			 * This is the toy for POS tagging.
			POSTagging(@"..\..\..\Corpus\TroFi.txt",
				@"..\..\..\Data\ProcessedCorpus\POSTag\TroFi.txt",
				"TroFi");
			 */

			Lemmatization(@"..\..\..\Data\ProcessedCorpus\POSTag\TroFi.txt", 
				@"..\..\..\Data\ProcessedCorpus\Lemmatization\TroFi.txt", 
				"TroFi");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace AIADemo
{
    class AIA
    {
        private int num_query;
        private string[] querys;
        private int NumPerQuery;
        private string pathImgDB;
        private int BinPerImg;
        private int bins;

        public AIA(int num_query, string[] querys, int NumPerQuery, string pathImgDB, int BinPerImg)
        {
            this.num_query = num_query;
            this.querys = querys;
            this.NumPerQuery = NumPerQuery;
            this.pathImgDB = pathImgDB;
            this.BinPerImg = BinPerImg;

            this.bins = BinPerImg * BinPerImg * BinPerImg; 
        }

        public double[] annoImg_weightedKNN(string file2Anno, float param_w)
        {
            FeatureExtracter fe = new FeatureExtracter(pathImgDB, BinPerImg, NumPerQuery);
            Bitmap bmp = new Bitmap(file2Anno);
            float[] feat = fe.getRGBFeature(bmp);
            float[] feat2 = null;
            double[] scores = new double[num_query];
            double totscore = 0;
            string querypath = null;

            //calculate the scores of the each query
            for (int i = 0; i < num_query; i++)
            {
                // textBox5.Text = textBox5.Text + i;
                querypath = pathImgDB + "\\" + querys[i];

                scores[i] = 0;
                for (int j = 1; j <= NumPerQuery; ++j)
                {
                    // textBox5.Text = textBox5.Text + j;
                    bmp = new Bitmap(querypath + "\\" + j + ".jpg");
                    feat2 = fe.getRGBFeature(bmp);

                    double eudist = 0;
                    for (int k = 0; k < bins; k++)
                        eudist += (feat[k] - feat2[k]) * (feat[k] - feat2[k]);
                    double gsdist = Math.Exp(0 - param_w * Math.Sqrt(eudist));

                    scores[i] += gsdist;
                }
                System.Console.WriteLine(scores[i]);
                totscore += scores[i];
            }

            for (int i = 0; i < num_query; i++)
                scores[i] /= totscore;
            return scores;
        }
    }
}

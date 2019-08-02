using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form2 : Form
    {


        public Form2()
        {
            InitializeComponent();

        }

        
        private int[] histogramme;
        public int[] Histogramme
        {
            get { return histogramme; }

        }

        private int[] histogrammeCumule;
        public int[] HistogrammeCumule
        {
            get { return histogrammeCumule; }
        }
        private float[] histogrammeEgal;
        public float[] HistogrammeEgal
        {
            get { return histogrammeEgal; }
        }

        public void AgrandirSizeImage(PictureBox pictureClicked)
        {
            if (pictureClicked.Image == null)
                return;

            pictureBox_AgrandirRetrissire.Image = pictureClicked.Image;
            label_Informations.Visible = false;
        }

        private void btnopen_Click(object sender, EventArgs e)
        {
            OpenFileDialog f = new OpenFileDialog();
            f.Filter = "Image File(*.bmp,*.jpg)|*. bmp;*.jpg";
            if (DialogResult.OK == f.ShowDialog())
            {
                textBox1.Text = f.FileName;
                this.pictureBox_Original.Image = new Bitmap(f.FileName);
            }
        }

        public Bitmap convertTGray(Bitmap b)
        {
            for (int k = 0; k < b.Width; k++)
            {
                for (int j = 0; j < b.Height; j++)
                {
                    Color c1 = b.GetPixel(k, j);
                    int r1 = c1.R;
                    int g1 = c1.G;
                    int b1 = c1.B;
                    int gray = (byte)(.299 * r1 + .587 * g1 + .114 * b1);
                    r1 = gray;
                    b1 = gray;
                    g1 = gray;
                    b.SetPixel(k, j, Color.FromArgb(r1, g1, b1));
                }
            }

            return b;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Bitmap copy = new Bitmap((Bitmap)this.pictureBox_Original.Image);
            copy = convertTGray(copy);
            this.pictureBox_NG.Image = copy;
        }

        private void btnsave_Click(object sender, EventArgs e)
        {
            SaveFileDialog sf = new SaveFileDialog();
            sf.Filter = "Image File(*.bmp,*.jpg)|*. bmp;*.jpg";
            if (DialogResult.OK == sf.ShowDialog())
            {
                //* File.saves(sf.FileName);
            }

        }

        private void calculerHistogramme(Bitmap b)
        {
            Color c;
            histogramme = new int[256];
            for (int i = 0; i < 256; i++)
            {
                histogramme[i] = 0;
            }


            for (int i = 0; i < b.Width; i++)  // colonne
            {
                for (int j = 0; j < b.Height; j++) // ligne
                {
                    c = b.GetPixel(i, j);
                    histogramme[(byte)c.R]++;
                }
            }


        }

        private void btnhisto_Click(object sender, EventArgs e)
        {

            histogramme = new int[256];

            //Calcul du Histogramme
            Bitmap b = new Bitmap((Bitmap)this.pictureBox_NG.Image);
            calculerHistogramme(b);


            for (int i = 1; i < 256; i++)
            {
                chart_Hist.Series["Series1"].Points.AddXY(i, histogramme[i]);
            }
            chart_Hist.Visible = true;
            /*TextBox txt = new TextBox();*/

        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {

        }

        private void chart2_Click(object sender, EventArgs e)
        {

        }

        public int[,] SegmentationParRegion(Bitmap image)
        {
            int[,] matriceImg = new int[image.Width,image.Height];
            Bitmap SegParRegion = new Bitmap(image.Width, image.Height);


            // Trouver le seuil 
            int seuil = 230;

            // Segmenter
            Color c;
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    if (j == 0 || j == image.Height-1)
                    {
                        matriceImg[i, j] = 0;
                        SegParRegion.SetPixel(i, j, Color.FromArgb(0, 0, 0));
                    }
                    else
                    {
                        if (i == 0 || i == image.Width-1)
                        {
                            matriceImg[i, j] = 0;
                            SegParRegion.SetPixel(i, j, Color.FromArgb(0, 0, 0));
                        }
                        else
                        {
                            c = image.GetPixel(i, j);
                            if (c.R < seuil)
                            {
                                matriceImg[i, j] = 0;
                                SegParRegion.SetPixel(i, j, Color.FromArgb(0, 0, 0));
                            }
                            else
                            {
                                matriceImg[i, j] = 1;
                                SegParRegion.SetPixel(i, j, Color.FromArgb(255, 255, 255));
                            }
                        }
                    }
                }
            }

            pictureBox_Detectage.Image = (SegParRegion);
            
            return matriceImg;
        }

        private Bitmap negative_algorithm(Bitmap bmap)
        {
            Color c;
            int[,] ImageData = new int[bmap.Width, bmap.Height];
            
            for (int i = 0; i < bmap.Width; i++)
            {
                for (int j = 0; j < bmap.Height; j++)
                {
                    c = bmap.GetPixel(i, j);
                    bmap.SetPixel(i, j, Color.FromArgb(255 - c.R, 255 - c.G, 255 - c.B));

                }
            }

            return bmap;
        }

        public void Detectage(Bitmap image)
        {
            // Variables
            Dictionary<int, int> references = new Dictionary<int, int>();
            int compteur_region = 0;

            int[,] matriceImg = SegmentationParRegion(image);

            // On gérera 3 cas :
            // cas 1 : les deux cases déjà visitées sont à zéro (0) qui veut dire que c'est une nouvelle région
            // cas 2 : une des deux cases déjà visitées est à zéro (0), qui veut dire que la case courante appartient
            // à une région déjà existante qui est représentée par la case non nulle.
            // cas 3 : les deux cases déjà visités ne sont pas à zéro (0) qui veut dire que deux régions sont 
            // similaires où on procédera à donner à la case courante la valeur minime entre les deux cases déjà
            // visitées puis de référencier dans la table des références la grande valeur à la petite valeur.

            for (int j = 1; j < image.Height - 1; j++)
                for (int i = 1; i < image.Width - 1; i++)
                {
                    if (matriceImg[i, j] == 0)
                        continue;

                    if (matriceImg[i - 1, j] == 0 && matriceImg[i, j - 1] == 0)
                    {   // Nouvelle région
                        matriceImg[i, j] = ++compteur_region;
                        continue;
                    }

                    if (matriceImg[i - 1, j] == 0)
                    {   // cas 2
                        matriceImg[i, j] = matriceImg[i, j - 1];
                        continue;
                    }

                    if (matriceImg[i, j - 1] == 0)
                    {   // cas 2
                        matriceImg[i, j] = matriceImg[i - 1, j];
                        continue;
                    }

                    // cas 3
                    // Il reste le cas où on prend le minimum des deux
                    if (matriceImg[i - 1, j] < matriceImg[i, j - 1])
                    {
                        matriceImg[i, j] = matriceImg[i - 1, j];
                        references[matriceImg[i, j - 1]] = matriceImg[i - 1, j];
                    }
                    else
                    {
                        matriceImg[i, j] = matriceImg[i, j - 1];
                        references[matriceImg[i, j - 1]] = matriceImg[i - 1, j];
                    }
                }
            
            Bitmap b_region = new Bitmap(image.Width, image.Height);

            for (int j = 0; j < image.Height; j++)
            {
                for (int i = 0; i < image.Width; i++)
                {
                    if (matriceImg[i, j] != 1)
                    {
                        if (references.ContainsKey(matriceImg[i, j]))
                            if (references[matriceImg[i, j]] == 1)
                            {
                                b_region.SetPixel(i, j, Color.FromArgb(matriceImg[i, j]));
                            }
                    }
                    else
                        b_region.SetPixel(i, j, Color.FromArgb(matriceImg[i, j]));
                }
            }

            pictureBox_Vide_3.Image = (b_region);
        }

        private void btn_normaliser_Click(object sender, EventArgs e)
        {
            int[] t = new int[256];
            Color c;
            for (int i = 0; i < 256; i++)
            {
                t[i] = 0;
            }

            Bitmap b3 = new Bitmap(this.pictureBox_NG.Image);
            for (int i = 0; i < b3.Width; i++)
            {
                for (int j = 0; j < b3.Height; j++)
                {
                    c = b3.GetPixel(i, j);
                    t[(byte)c.R]++;
                }
            }
            Console.WriteLine("l histogramme:" + t[3]);
            float nbr_pixel;

            nbr_pixel = b3.Width * b3.Height;
            Console.WriteLine("w*h : " + nbr_pixel);
            /*
                        Decimal[] t2 = new Decimal[256];

                        for (int i = 0; i < 256; i++)
                        {
                            Console.WriteLine("le ti : " + t[i]);


                            t2[i] = (Decimal) 1.0000 * (t[i] / nbr_pixel);
                            Console.WriteLine("le t2i : " + t2[i]);

                        }
                        */

            for (int i = 1; i < 256; i++)
            {

                chart_Normalise.Series["Series2"].Points.AddXY(i, t[i] / nbr_pixel);
            }
            chart_Normalise.Visible = true;
            /*Console.WriteLine("le nbre de pixel est:" + nbr);*/




        }

        private void chart3_Click(object sender, EventArgs e)
        {

        }

        public void calculerHistogrammeCumule(int[] histogramme)
        {
            int k = 0;
            histogrammeCumule = new int[256];
            HistogrammeCumule[0] = histogramme[0];
            for (int i = 1; i < 256; i++)
            {
                histogrammeCumule[i] = histogrammeCumule[k] + histogramme[i];
                k = k + 1;
            }
        }

        private void btn_cumulé_Click(object sender, EventArgs e)
        {
            Bitmap b4 = new Bitmap((Bitmap)this.pictureBox_Original.Image);

            if (histogramme == null)
            {
                calculerHistogramme(b4);
            }

            calculerHistogrammeCumule(histogramme);

            for (int i = 1; i < 256; i++)
            {
                chart_Cumule.Series["Series1"].Points.AddXY(i, histogrammeCumule[i]);
            }
            chart_Cumule.Visible = true;

        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        public Bitmap expension(Bitmap copy2)                          //expansion
        {

            Color c;
            c = copy2.GetPixel(0, 0);
            int minR = (byte)c.R;
            int minB = (byte)c.B;
            int minG = (byte)c.G;
            int maxR = (byte)c.R;
            int maxB = (byte)c.B;
            int maxG = (byte)c.G;

            for (int i = 0; i < copy2.Width; i++)
            {
                for (int j = 0; j < copy2.Height; j++)
                {
                    c = copy2.GetPixel(i, j);
                    // t[(byte)c.R]++;
                    if (minR > (byte)c.R)
                    {
                        minR = (byte)c.R;
                    }
                    if (minB > (byte)c.B)
                    {
                        minB = (byte)c.B;
                    }
                    if (minG > (byte)c.G)
                    {
                        minG = (byte)c.G;
                    }
                    if (maxR < (byte)c.R)
                    {
                        maxR = (byte)c.R;
                    }
                    if (maxB < (byte)c.B)
                    {
                        maxB = (byte)c.B;
                    }
                    if (maxG < (byte)c.G)
                    {
                        maxG = (byte)c.G;
                    }


                }
            }
            Color c2;
            for (int i = 0; i < copy2.Width; i++)
            {
                for (int j = 0; j < copy2.Height; j++)
                {
                    c = copy2.GetPixel(i, j);

                    byte n1 = (byte)(255 / (maxR - minR) * ((byte)c.R - minR));
                    byte n2 = (byte)(255 / (maxB - minB) * ((byte)c.B - minB));
                    byte n3 = (byte)(255 / (maxG - minG) * ((byte)c.G - minG));

                    copy2.SetPixel(i, j, Color.FromArgb(n1, n3, n2));

                }
            }

            return copy2;
        }

        private void btn4_Click(object sender, EventArgs e)                    //btn expension
        {
            Bitmap copy2 = new Bitmap((Bitmap)this.pictureBox_Original.Image);
            copy2 = expension(copy2);
            this.pictureBox_Expension.Image = copy2;

        }


        private void pictureBox3_Click_1(object sender, EventArgs e)
        {

        }

        public Bitmap moyenneur(Bitmap b)
        {
            int N = 7;
            int pg;
            int pr;
            int pb;
            int[,] c = new int[N, N];
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    c[i, j] = 1;
                }
            }


            int f = (int)(N / 2);
            int l = -(int)(N / 2);
            int k = -(int)(N / 2);
            int sg = 0;
            int sr = 0;
            int sb = 0;
            
            for (int i = f; i < b.Width - f; i++)
            {
                for (int j = f; j < b.Height - f; j++)
                {
                    //Color c2 = b.GetPixel(i, j);
                    sg = 0;
                    sr = 0;
                    sb = 0;


                    // I(x−i, y−j).H(i, j)

                    for (int z = k; z <= f; z++)
                    {
                        for (int nb = l; nb <= f; nb++)
                        {
                            Color c3 = b.GetPixel(i + z, j + nb);
                            pr = (byte)c3.R;
                            pg = (byte)c3.G;
                            pb = (byte)c3.B;

                            sg = sg + pg * c[f + z, f + nb];
                            sr = sr + pr * c[f + z, f + nb];
                            sb = sb + pb * c[f + z, f + nb];
                        }

                    }

                    sr = sr / (N * N);
                    sg = sg / (N * N);
                    sb = sb / (N * N);
                    if (sr < 0) { sr = 0; }
                    if (sg < 0) { sg = 0; }
                    if (sb < 0) { sb = 0; }
                    if (sr > 255) { sr = 255; }
                    if (sg > 255) { sg = 255; }
                    if (sb > 255) { sb = 255; }
                    b.SetPixel(i, j, Color.FromArgb(sr, sg, sb));
                }

            }

            return b;
        }

        private void btn6_Click(object sender, EventArgs e)
        {
            Bitmap copy3 = new Bitmap((Bitmap)this.pictureBox_Original.Image);
            copy3 = moyenneur(copy3);
            this.pictureBox_Moyenneur.Image = copy3;
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {

        }

        private Point[] positions = new Point[11] { new Point(3, 430), new Point(240, 431), new Point(474,430), new Point(711, 430),
                                                new Point(113, 140), new Point(94, 118), new Point(72, 92), new Point(55, 62),
                                                new Point(43, 45), new Point(25, 23),
                                                new Point(4, 12)};

        private void RightButtonPictureBoxes()
        {
            int nombrePicBoxes = 11;
            List<Panel> list_panels = new List<Panel>() {panel_Expension, panel_Gaussien, panel_Mediant,
                panel_Egalisation, panel_Moyenneur, panel_NG, panel_SegContour, panel_AugContraste, panel_SegReg, panel_Vide_3,
                panel_Convolution};

            for (int i = 0; i < nombrePicBoxes; i++)
            {
                switch (list_panels[i].Location.X)
                {
                    case 3: // Take to the end. Position 11 and Change its size
                        list_panels[i].Location = positions[1];
                        break;
                    case 240:
                        list_panels[i].Location = positions[2];
                        break;
                    case 474:
                        list_panels[i].Location = positions[3];
                        break;
                    case 711:
                        list_panels[i].Location = positions[4];
                        break;
                    case 113:
                        list_panels[i].Location = positions[5];
                        break;
                    case 94:
                        list_panels[i].Location = positions[6];
                        break;
                    case 72:
                        list_panels[i].Location = positions[7];
                        break;
                    case 55:
                        list_panels[i].Location = positions[8];
                        break;
                    case 43:
                        list_panels[i].Location = positions[9];
                        break;
                    case 25:
                        list_panels[i].Location = positions[10];
                        break;
                    case 4:
                        list_panels[i].Location = positions[0];
                        break;

                    default:
                        break;
                }

            }
        }


        private void LeftButtonPictureBoxes()
        {
            int nombrePicBoxes = 11;
            List<Panel> list_panels = new List<Panel>() {panel_Expension, panel_Gaussien, panel_Mediant,
                panel_Egalisation, panel_Moyenneur, panel_NG, panel_SegContour, panel_AugContraste, panel_SegReg, panel_Vide_3,
                panel_Convolution};


            for (int i = 0; i < nombrePicBoxes; i++)
            {
                switch(list_panels[i].Location.X)
                {
                    case 3: // Take to the end. Position 11 and Change its size
                        list_panels[i].Location = positions[10];
                        break;
                    case 240:
                        list_panels[i].Location = positions[0];
                        break;
                    case 474:
                        list_panels[i].Location = positions[1];
                        break;
                    case 711:
                        list_panels[i].Location = positions[2];
                        break;
                    case 113:
                        list_panels[i].Location = positions[3];
                        break;
                    case 94:
                        list_panels[i].Location = positions[4];
                        break;
                    case 72:
                        list_panels[i].Location = positions[5];
                        break;
                    case 55:
                        list_panels[i].Location = positions[6];
                        break;
                    case 43:
                        list_panels[i].Location = positions[7];
                        break;
                    case 25:
                        list_panels[i].Location = positions[8];
                        break;
                    case 4:
                        list_panels[i].Location = positions[9];
                        break;

                    default:
                        break;
                }

            }
        }


        private void button2_Click(object sender, EventArgs e)
        {
            Bitmap copy4 = new Bitmap((Bitmap)this.pictureBox_Original.Image);
            copy4 = egalisation(copy4);
            this.pictureBox_Egalisation.Image = copy4;

        }

        public float maximumMatrice(Bitmap matrice)
        {
            int maxValue = matrice.GetPixel(0, 0).ToArgb();
            int currentValue;

            for (int i = 0; i < matrice.Width; i++)
                for (int j = 0; j < matrice.Height; j++)
                {
                    currentValue = matrice.GetPixel(i, j).ToArgb();
                    if (maxValue < currentValue)
                    {
                        maxValue = currentValue;
                    }
                }


            return (float)maxValue;
        }

        public Bitmap egalisation(Bitmap b3)
        {
            // hega = max(I) * histcum / (w*h)
            Bitmap bitEgalised = new Bitmap(b3.Width, b3.Height);

            //Calcul du Histogramme
            if (histogramme == null)
            {
                calculerHistogramme(b3);
            }
            if (histogrammeCumule == null)
            {
                calculerHistogrammeCumule(histogramme);
            }

            Color c;
            c = b3.GetPixel(0, 0);
            float max = (byte)c.R;
            for (int i = 0; i < b3.Width; i++)
            {
                for (int j = 0; j < b3.Height; j++)
                {
                    c = b3.GetPixel(i, j);

                    if (max < (byte)c.R)
                    {
                        max = (byte)c.R;
                    }

                }
            }
            float maxMatrice = max;


            histogrammeEgal = new float[256];

            for (int i = 0; i < 256; i++)
            {
                float value = b3.Height * b3.Width;

                histogrammeEgal[i] = histogrammeCumule[i] * maxMatrice / value;
                //Console.WriteLine("hqhqhq     " + i + "   " + histogrammeEgal[i]);
            }

            for (int i = 0; i < b3.Width; i++)
                for (int j = 0; j < b3.Height; j++)
                {
                    c = b3.GetPixel(i, j);
                    byte iegal = (byte)histogrammeEgal[c.R];
                    bitEgalised.SetPixel(i, j, Color.FromArgb(iegal, iegal, iegal));
                }



            return bitEgalised;
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        public Bitmap AUGMENTATION_CONTRASTE(Bitmap b)
        {
            int N = 3;
            int pg;
            int pr;
            int pb;
            int[,] c = new int[N, N];
            c[0, 0] = 0;
            c[0, 1] = 1;
            c[0, 2] = 0;
            c[1, 0] = -1;
            c[1, 1] = 5;
            c[1, 2] = -1;
            c[2, 0] = 0;
            c[2, 1] = -1;
            c[2, 2] = 0;

            int f = (int)(N / 2);
            int l = -(int)(N / 2);
            int k = -(int)(N / 2);
            int sg = 0;
            int sr = 0;
            int sb = 0;



            for (int i = f; i < b.Width - f; i++)
            {
                for (int j = f; j < b.Height - f; j++)
                {
                    //Color c2 = b.GetPixel(i, j);
                    sg = 0;
                    sr = 0;
                    sb = 0;



                    for (int z = k; z <= f; z++)
                    {
                        for (int nb = l; nb <= f; nb++)
                        {
                            Color c3 = b.GetPixel(i + z, j + nb);
                            pr = (byte)c3.R;
                            pg = (byte)c3.G;
                            pb = (byte)c3.B;
                            //Console.WriteLine((f+z) + " coucouuuu " + (f+nb));
                            sg = sg + pg * c[f + z, f + nb];
                            sr = sr + pr * c[f + z, f + nb];
                            sb = sb + pb * c[f + z, f + nb];
                        }

                    }
                    sr = sr / (N * N);
                    sg = sg / (N * N);
                    sb = sb / (N * N);
                    if (sr < 0) { sr = 0; }
                    if (sg < 0) { sg = 0; }
                    if (sb < 0) { sb = 0; }
                    if (sr > 255) { sr = 255; }
                    if (sg > 255) { sg = 255; }
                    if (sb > 255) { sb = 255; }
                    b.SetPixel(i, j, Color.FromArgb(sr, sg, sb));
                }

            }

            return b;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Bitmap copy6 = new Bitmap((Bitmap)this.pictureBox_Original.Image);
            copy6 = AUGMENTATION_CONTRASTE(copy6);

            this.pictureBox_AugContraste.Image = copy6;
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {

        }

        public Bitmap segmentation_contour(Bitmap b)
        {
            Bitmap grad = new Bitmap((Bitmap)b);
            int N = 7;
            int pg;
            int pr;
            int pb;
            int[,] cx = new int[N, N];
            int[,] cy = new int[N, N];
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    if (j == 0) { cx[i, j] = 1; }
                    if (j == 1) { cx[i, j] = 0; }
                    if (j == 2) { cx[i, j] = -1; }
                    if (i == 0) { cy[i, j] = 1; }
                    if (i == 1) { cy[i, j] = 0; }
                    if (i == 2) { cy[i, j] = -1; }
                }
            }

            int f = (int)(N / 2);
            int l = -(int)(N / 2);
            int k = -(int)(N / 2);
            int sg = 0;
            int sr = 0;
            int sb = 0;
            int sg2 = 0;
            int sr2 = 0;
            int sb2 = 0;
            int seuil = 200;


            for (int i = f; i < b.Width - f; i++)
            {
                for (int j = f; j < b.Height - f; j++)
                {

                    sg = 0;
                    sr = 0;
                    sb = 0;
                    sg2 = 0;
                    sr2 = 0;
                    sb2 = 0;
                    for (int z = k; z <= f; z++)
                    {
                        for (int nb = l; nb <= f; nb++)
                        {
                            Color c3 = b.GetPixel(i + z, j + nb);
                            pg = (byte)c3.R;
                            pr = (byte)c3.G;
                            pb = (byte)c3.B;

                            sg = sg + pg * cx[f + z, f + nb];
                            sr = sr + pr * cx[f + z, f + nb];
                            sb = sb + pb * cx[f + z, f + nb];
                            sg2 = sg2 + pg * cy[f + z, f + nb];
                            sr2 = sr2 + pr * cy[f + z, f + nb];
                            sb2 = sb2 + pb * cy[f + z, f + nb];
                        }
                    }

                    sr = sr / (N * N);
                    sg = sg / (N * N);
                    sb = sb / (N * N);
                    sr2 = sr2 / (N * N);
                    sg2 = sg2 / (N * N);
                    sb2 = sb2 / (N * N);
                    if (sr < 0) { sr = 0; }
                    if (sg < 0) { sg = 0; }
                    if (sb < 0) { sb = 0; }
                    if (sr > 255) { sr = 255; }
                    if (sg > 255) { sg = 255; }
                    if (sb > 255) { sb = 255; }
                    if (sr2 < 0) { sr2 = 0; }
                    if (sg2 < 0) { sg2 = 0; }
                    if (sb2 < 0) { sb2 = 0; }
                    if (sr2 > 255) { sr2 = 255; }
                    if (sg2 > 255) { sg2 = 255; }
                    if (sb2 > 255) { sb2 = 255; }

                    int a2 = (int)Math.Sqrt(((sg * sg) + (sg2 * sg2)));
                    int a3 = (int)Math.Sqrt(((sb * sb) + (sb2 * sb2)));
                    int a = (int)Math.Sqrt(((sr * sr) + (sr2 * sr2)));
                    grad.SetPixel(i, j, Color.FromArgb(a, a2, a3));
                    Color cc;
                    cc = b.GetPixel(i, j);
                    int pp1 = (byte)cc.R;
                    int pp2 = (byte)cc.G;
                    int pp3 = (byte)cc.B;
                    if (pp1 <= seuil) { pp1 = 0; }
                    if (pp1 > seuil) { pp1 = 255; }
                    if (pp2 <= seuil) { pp2 = 0; }
                    if (pp2 > seuil) { pp2 = 255; }
                    if (pp3 <= seuil) { pp3 = 0; }
                    if (pp3 > seuil) { pp3 = 255; }
                    b.SetPixel(i, j, Color.FromArgb(pp1, pp2, pp3));
                }

            }

            return b;
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            Bitmap copy7 = new Bitmap((Bitmap)this.pictureBox_NG.Image);
            copy7 = segmentation_contour(copy7);
            this.pictureBox_SegContour.Image = copy7;
        }

        private void pictureBox7_Click(object sender, EventArgs e)
        {

        }

        public Bitmap gaussien(Bitmap b)
        {
            int N = 5;
            float nn = (float)(N * N);
            float pg;
            double sigma = (double)0.6;

            double[,] c = new double[N, N];
            
            double test;
            int taille = N;

            for (int i = 0; i < taille; i++)
            {
                for (int j = 0; j < taille; j++)
                {
                    test = (1 / (2 * Math.PI * Math.Pow(sigma, 2))) * Math.Exp((double)(-(Math.Pow((i - (taille / 2)), 2) + Math.Pow((j - (taille / 2)), 2)) / (2 * Math.Pow(sigma, 2))));
                    c[i, j] = test;
                }
            }


            int f = (int)(N / 2);
            int l = -(int)(N / 2);
            int k = -(int)(N / 2);
            float sg;
            
            for (int i = f; i < b.Width - f; i++)
            {
                for (int j = f; j < b.Height - f; j++)
                {
                    sg = 0;
                    
                    for (int z = k; z <= f; z++)
                    {
                        for (int nb = l; nb <= f; nb++)
                        {
                            Color c3 = b.GetPixel(i + z, j + nb);

                            pg = (byte)c3.G;
                            sg = (float)(sg + (float)(pg * (float)c[f + z, f + nb]));
                        }

                    }
                    b.SetPixel(i, j, Color.FromArgb((int)sg, (int)sg, (int)sg));
                }

            }

            return b;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Bitmap copy8 = new Bitmap((Bitmap)this.pictureBox_NG.Image);
            copy8 = gaussien(copy8);
            this.pictureBox_Gaussien.Image = copy8;
        }

        private void pictureBox8_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox9_Click(object sender, EventArgs e)
        {

        }

        public Bitmap mediant(Bitmap b)
        {
            int N = 7;
            int[] tabr = new int[N * N];
            int[] tabg = new int[N * N];
            int[] tabb = new int[N * N];
            int f = (int)(N / 2);
            int l = -(int)(N / 2);
            int k = -(int)(N / 2);



            for (int i = f; i < b.Width - f; i++)
            {
                for (int j = f; j < b.Height - f; j++)
                {

                    int cpt = 0;
                    for (int z = k; z <= f; z++)
                    {
                        for (int nb = l; nb <= f; nb++)
                        {
                            Color c3 = b.GetPixel(i + z, j + nb);
                            tabr[cpt] = (byte)c3.R;
                            tabg[cpt] = (byte)c3.G;
                            tabb[cpt] = (byte)c3.B;
                            cpt++;
                        }
                    }

                    Array.Sort(tabr);
                    Array.Sort(tabg);
                    Array.Sort(tabb);

                    int w = (int)N * N / 2;
                    b.SetPixel(i, j, Color.FromArgb(tabr[w], tabg[w], tabb[w]));
                }
            }

            return b;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Bitmap copy9 = new Bitmap((Bitmap)this.pictureBox_Original.Image);
            copy9 = mediant(copy9);
            this.pictureBox_Mediant.Image = copy9;

        }

        private void Form2_Load(object sender, EventArgs e)
        {
            
        }

        private void button_redimessionnement_Click(object sender, EventArgs e)
        {
            pictureBox_AgrandirRetrissire.Image = null;
            label_Informations.Visible = true;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            LeftButtonPictureBoxes();
        }

        private void button7_Click_1(object sender, EventArgs e)
        {
            RightButtonPictureBoxes();
        }

        private void pictureBox_NG_DoubleClick(object sender, EventArgs e)
        {
            AgrandirSizeImage((PictureBox)sender);
        }

        private void pictureBox_Moyenneur_DoubleClick(object sender, EventArgs e)
        {
            AgrandirSizeImage((PictureBox)sender);
        }

        private void pictureBox_Gaussien_DoubleClick(object sender, EventArgs e)
        {
            AgrandirSizeImage((PictureBox)sender);
        }

        private void pictureBox_Egalisation_DoubleClick(object sender, EventArgs e)
        {
            AgrandirSizeImage((PictureBox)sender);
        }

        private void pictureBox_Expension_DoubleClick(object sender, EventArgs e)
        {
            AgrandirSizeImage((PictureBox)sender);
        }

        private void pictureBox_Mediant_DoubleClick(object sender, EventArgs e)
        {
            AgrandirSizeImage((PictureBox)sender);
        }

        private void pictureBox_SegContour_DoubleClick(object sender, EventArgs e)
        {
            AgrandirSizeImage((PictureBox)sender);
        }

        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            AgrandirSizeImage((PictureBox)sender);
        }

        private void pictureBox2_DoubleClick(object sender, EventArgs e)
        {
            AgrandirSizeImage((PictureBox)sender);
        }

        private void pictureBox3_DoubleClick(object sender, EventArgs e)
        {
            AgrandirSizeImage((PictureBox)sender);
        }

        private void pictureBox6_DoubleClick(object sender, EventArgs e)
        {
            AgrandirSizeImage((PictureBox)sender);
        }

        private void button_Hists_FullImg_Click(object sender, EventArgs e)
        {
            pictureBox_AgrandirRetrissire.Visible = !pictureBox_AgrandirRetrissire.Visible;
            label_Informations.Visible = !label_Informations.Visible;

            chart_Cumule.Visible = !chart_Cumule.Visible;
            chart_Hist.Visible = !chart_Hist.Visible;
            chart_Normalise.Visible = !chart_Normalise.Visible;
        }

        private void button_Detectage_Click(object sender, EventArgs e)
        {
            Detectage((Bitmap)pictureBox_NG.Image);
        }

        private void pictureBox_NG_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox_Gaussien_Click(object sender, EventArgs e)
        {

        }

        public Bitmap convolution(Bitmap b)
        {

            int N = 3;
            int pg;
            int pr;
            int pb;
            int[,] c = new int[N, N];
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    c[i, j] = 1;
                    
                }
            }
            int f = (int)(N / 2);
            int l = -(int)(N / 2);
            int k = -(int)(N / 2);
            int sg = 0;
            int sr = 0;
            int sb = 0;



            for (int i = 1; i < b.Width - 1; i++)
            {
                for (int j = 1; j < b.Height - 1; j++)
                {

                    sg = 0;
                    sr = 0;
                    sb = 0;


                    int z = k;
                    while (z <= f)
                    {
                        for (int nb = l; nb <= f; nb++)
                        {
                            
                            Color c3 = b.GetPixel(i + z, j + nb);
                            pr = (byte)c3.R;
                            pg = (byte)c3.G;
                            pb = (byte)c3.B;
                            sg = sg + pg * c[z + f, nb + f];
                            sr = sr + pr * c[z + f, nb + f];
                            sb = sr + pb * c[z + f, nb + f];
                            
                        }

                        z = z + 1;

                    }
                    sr = sr / (N * N);
                    sg = sg / (N * N);
                    sb = sb / (N * N);

                    if (sg > 255)
                    {
                        sg = 255;
                    }
                    if (sr > 255)
                    {
                        sr = 255;
                    }
                    if (sb > 255)
                    {
                        sb = 255;
                    }
                    if (sg < 0)
                    {
                        sg = 0;
                    }
                    if (sr < 0)
                    {
                        sr = 0;
                    }
                    if (sb < 0)
                    {
                        sb = 0;
                    }
                    
                    b.SetPixel(i, j, Color.FromArgb(sr, sg, sb));
                }
            }

            return b;
        }

        private void button_Convolution_Click(object sender, EventArgs e)
        {
            Bitmap copy = new Bitmap((Bitmap)this.pictureBox_NG.Image);
            copy = convolution(copy);
            this.pictureBox_Convolution.Image = copy;
        }
    }

    public class region
    {
        public region()
        {
            MoyenneRegion = 0f;
            Entrepot = new List<int>();
        }

        public float MoyenneRegion { get; private set; }
        public List<int> Entrepot { get; }

        public void AddValueToRegion(int value)
        {
            Entrepot.Add(value);
            int moy = 0;
            foreach (int val in Entrepot)
            {
                moy += val;
            }

            MoyenneRegion = moy / Entrepot.Count;
        }

        public void RemoveValueFromRegion(int value)
        {
            Entrepot.Remove(value);
            int moy = 0;
            foreach (int val in Entrepot)
            {
                moy += val;
            }

            MoyenneRegion = moy / Entrepot.Count;
        }

    }

}







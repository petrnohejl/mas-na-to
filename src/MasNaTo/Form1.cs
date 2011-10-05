using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace MasNaTo
{
    public partial class Form1 : Form
    {
        private int players;
        private int time;
        private ArrayList words;
        private int word;
        private int[] score;
        private int modificator;
        private string[] modif = new string[] { "1x", "1x", "2x", "+1", "-1", "žolik" };
        private int timecounter;
        private System.Media.SoundPlayer snd = new System.Media.SoundPlayer("alert.wav");

        public Form1()
        {
            InitializeComponent();
            LoadDatabase();

            this.comboBox2.SelectedIndex = 0;
        }
        
        private void button3_Click(object sender, EventArgs e)
        {
            if (this.button1.Text != "Stop") Application.Exit();
            else
                if(MessageBox.Show("Opravdu chcete ukončit aplikaci?", "Konec", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // zapnuti hry
            if (this.button1.Text == "Start")
            {
                bool error = false;

                // kontrola nastaveni databaze
                string s = this.comboBox1.GetItemText(this.comboBox1.SelectedItem);
                if (!File.Exists(s))
                {
                    MessageBox.Show("Zvolená databáze neexistuje!", "Chyba databáze", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    error = true;
                }

                // kontrola intervalu
                if (this.numericUpDown1.Value > this.numericUpDown2.Value)
                {
                    MessageBox.Show("Časový interval je špatně nastaven!\nNastavte interval v rozmezí 5 - 100.", "Chyba nastavení", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    error = true;
                }

                // kontrola hracu
                if(this.checkBox1.Checked && this.textBox1.Text == "")
                {
                    MessageBox.Show("Hráč 1 nemá jméno!", "Chyba nastavení", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    error = true;
                }

                if (this.checkBox2.Checked && this.textBox2.Text == "")
                {
                    MessageBox.Show("Hráč 2 nemá jméno!", "Chyba nastavení", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    error = true;
                }

                if (this.checkBox3.Checked && this.textBox3.Text == "")
                {
                    MessageBox.Show("Hráč 3 nemá jméno!", "Chyba nastavení", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    error = true;
                }

                if (this.checkBox4.Checked && this.textBox4.Text == "")
                {
                    MessageBox.Show("Hráč 4 nemá jméno!", "Chyba nastavení", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    error = true;
                }

                // pocet hracu
                this.players = 0;
                if (this.checkBox1.Checked) this.players++;
                if (this.checkBox2.Checked) this.players++;
                if (this.checkBox3.Checked) this.players++;
                if (this.checkBox4.Checked) this.players++;
                if (players < 2 || !this.checkBox1.Checked || !this.checkBox2.Checked)
                {
                    MessageBox.Show("Hru musí hrát alespoň 2 hráči!", "Chyba nastavení", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    error = true;
                }

                // nenastala chyba ergo startuji hru
                if (error == false)
                {
                    this.button1.Text = "Stop";
                    this.groupBox2.Location = new Point(12,12);
                    this.groupBox1.Visible = false;
                    this.groupBox2.Visible = true;

                    // skryti nepouzitych hracu
                    if (!this.checkBox1.Checked)
                    {
                        this.radioButton1.Visible = false;
                        this.label7.Visible = false;
                    }
                    if (!this.checkBox2.Checked)
                    {
                        this.radioButton2.Visible = false;
                        this.label8.Visible = false;
                    }
                    if (!this.checkBox3.Checked)
                    {
                        this.radioButton3.Visible = false;
                        this.label9.Visible = false;
                    }
                    if (!this.checkBox4.Checked)
                    {
                        this.radioButton4.Visible = false;
                        this.label10.Visible = false;
                    }

                    // modifikator
                    if (!this.checkBox5.Checked)
                        label15.Visible = false;

                    // jmena hracu
                    this.radioButton1.Text = this.textBox1.Text;
                    this.radioButton2.Text = this.textBox2.Text;
                    this.radioButton3.Text = this.textBox3.Text;
                    this.radioButton4.Text = this.textBox4.Text;

                    // body
                    this.label7.Text = "0 bodů";
                    this.label8.Text = "0 bodů";
                    this.label9.Text = "0 bodů";
                    this.label10.Text = "0 bodů";

                    // ostatni ukazatele
                    this.progressBar1.Value = 0;
                    this.label5.Text = "0:00";
                    this.label6.Text = "1. ???";
                    this.label15.Text = "Modifikátor:";
                    this.numericUpDown3.Value = 0;
                    this.numericUpDown4.Value = 0;

                    // enable prvky
                    label13.Enabled = true;
                    label14.Enabled = true;
                    button4.Enabled = true;
                    button6.Enabled = true;
                    numericUpDown4.Enabled = true;

                    // disable prvky
                    label11.Enabled = false;
                    label12.Enabled = false;
                    label15.Enabled = false;
                    numericUpDown3.Enabled = false;
                    button5.Enabled = false;

                    // nacteni slov
                    LoadWords();

                    // score
                    this.score = new int[] { 0, 0, 0, 0 };

                    // word
                    this.word = 0;

                    // times up
                    timecounter = 0;

                    // status bar
                    this.toolStripStatusLabel1.Text = "Databáze: " + comboBox1.Text + " (" + words.Count + " slov)          ";
                    if (this.comboBox2.SelectedIndex == 0) this.toolStripStatusLabel2.Text = "Bez limitu";
                    else if (this.comboBox2.SelectedIndex == 1) this.toolStripStatusLabel2.Text = "Zbývá: " + (int)numericUpDown5.Value + " minut";
                    else if (this.comboBox2.SelectedIndex == 2) this.toolStripStatusLabel2.Text = "Zbývá: " + (int)numericUpDown5.Value + " kol";

                    // timer
                    if (this.comboBox2.SelectedIndex == 1)
                        timer2.Start();
                }
            }
            else
            {
                DialogResult res;
                if (this.button1.Text == "Stop")
                {
                    res = MessageBox.Show("Opravdu chcete ukončit hru?", "Konec hry", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                }
                else
                {
                    res = DialogResult.Yes;
                }
                
                if (res == DialogResult.Yes)
                {
                    this.button1.Text = "Start";
                    this.groupBox1.Visible = true;
                    this.groupBox2.Visible = false;

                    this.radioButton1.Visible = true;
                    this.label7.Visible = true;

                    this.radioButton2.Visible = true;
                    this.label8.Visible = true;

                    this.radioButton3.Visible = true;
                    this.label9.Visible = true;

                    this.radioButton4.Visible = true;
                    this.label10.Visible = true;

                    this.label15.Visible = true;

                    this.toolStripStatusLabel1.Text = " ";
                    this.toolStripStatusLabel2.Text = " ";

                    timer1.Stop();

                    if (this.comboBox2.SelectedIndex == 1)
                        timer2.Stop();
                }
            }
        }

        private void LoadDatabase()
        {
            string[] files = Directory.GetFiles(".", "*.txt");
            foreach (string s in files)
            {
                this.comboBox1.Items.Add(s);
            }
            if (this.comboBox1.Items.Count > 0) this.comboBox1.SelectedIndex = 0;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if ((int)numericUpDown4.Value > 0)
            {
                // nahodny cas
                Random random = new Random();
                this.time = random.Next((int)this.numericUpDown1.Value, (int)this.numericUpDown2.Value + 1) * 10;

                // progress bar
                this.progressBar1.Maximum = this.time;
                this.progressBar1.Value = 0;

                // vypis slova
                int cnt = 0;
                foreach (string s in words)
                {
                    if (cnt == word)
                    {
                        label6.Text = ++this.word + ". " + s;
                        break;
                    }
                    cnt++;
                }

                // disable prvky
                button4.Enabled = false;
                label13.Enabled = false;
                numericUpDown4.Enabled = false;
                label14.Enabled = false;

                // timer
                timer1.Start();
                System.Media.SystemSounds.Beep.Play();
            }
            else
            {
                MessageBox.Show("Nastavte pole licitace!", "Chyba licitace", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (this.time > 0)
            {
                this.time -= 1;
                label5.Text = (this.time / 10).ToString() + ":" + (this.time % 10).ToString() + "0";
                this.progressBar1.Value += 1;
            }
            else
            {
                timer1.Stop();
                this.snd.Play();

                // enable prvky
                label11.Enabled = true;
                label12.Enabled = true;
                numericUpDown3.Enabled = true;
                button5.Enabled = true;
                label15.Enabled = true;

                // cas
                label5.Text = "0:00";

                // nasobitel
                Random random = new Random();
                this.modificator = random.Next(0, 6);
                label15.Text = "Modifikátor: " + this.modif[modificator];
            }
        }

        private void LoadWords()
        {
            words = new ArrayList();
            ArrayList wordsTmp = new ArrayList();
            Random rand = new Random();
            words.Clear();
            wordsTmp.Clear();

            // cteni souboru
            FileStream fStream = new FileStream(comboBox1.Text, FileMode.Open, FileAccess.Read, FileShare.Read);
            StreamReader sr = new StreamReader(fStream, Encoding.UTF8, true);
            while (sr.Peek() > -1)
            {
                string input = sr.ReadLine();
                wordsTmp.Add(input);
            }
            sr.Close();

            // random prehazeni slov
            while (wordsTmp.Count > 0)
            {
                int num = rand.Next(0, wordsTmp.Count);
                words.Add(wordsTmp[num]);
                wordsTmp.RemoveAt(num);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // aktivni hrac
            int player = 0;
            string name = "";
            if (radioButton1.Checked)
            { 
                player = 0;
                name = radioButton1.Text;
            }
            else if (radioButton2.Checked) 
            {
                player = 1;
                name = radioButton2.Text;
            }
            else if (radioButton3.Checked) 
            {
                player = 2;
                name = radioButton3.Text;
            }
            else if (radioButton4.Checked) 
            { 
                player = 3;
                name = radioButton4.Text;
            }

            // vyhodnoceni ziskanych bodu { "1x", "1x", "2x", "+1", "-1", "žolik" }
            int points = 0;
            int licit = (int)numericUpDown4.Value;
            int said = (int)numericUpDown3.Value;
            
            // hra s modifikatorem
            if (checkBox5.Checked)
            {
                switch (this.modificator)
                {
                    case 0: // *1
                        if (said < licit) points -= licit; // prohral
                        else points += licit; // vyhral
                        break;
                    case 1: // *1
                        if (said < licit) points -= licit; // prohral
                        else points += licit; // vyhral
                        break;
                    case 2: // *2
                        if (said < licit) points -= licit; // prohral
                        else points += licit * 2; // vyhral
                        break;
                    case 3: // +1
                        if (said + 1 < licit) points -= licit; // prohral
                        else points += licit; // vyhral
                        break;
                    case 4: // -1
                        if (said - 1 < licit) points -= licit; // prohral
                        else points += licit; // vyhral
                        break;
                    case 5: // zolik
                        points += 12;
                        break;
                }
            }
            else
            {
                if (said < licit) points -= licit; // prohral
                else points += licit; // vyhral
            }
           
            // dialog pricteni bodu
            string mdf = "";
            if(checkBox5.Checked) mdf = "magický modifikátor byl " + this.modif[this.modificator] + ", ";
            string msg = "Hráč " + name + " licitoval " + licit + " slov, správně uhodnul " + said + " slov,\n" + mdf + "tudíš získává " + points + " bodů.";
            if (MessageBox.Show(msg, "Zapsat body", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                // pricteni score
                score[player] += points;
                label7.Text = score[0] + " bodů";
                label8.Text = score[1] + " bodů";
                label9.Text = score[2] + " bodů";
                label10.Text = score[3] + " bodů";

                // enable prvky
                button4.Enabled = true;
                label13.Enabled = true;
                numericUpDown4.Enabled = true;
                label14.Enabled = true;

                // disable prvky
                label11.Enabled = false;
                label12.Enabled = false;
                numericUpDown3.Enabled = false;
                button5.Enabled = false;
                label15.Enabled = false;

                // progress bar
                progressBar1.Value = 0;

                // snulovani pole
                numericUpDown3.Value = 0;
                numericUpDown4.Value = 0;

                // label
                label6.Text = (this.word+1) + ". ???";
                label15.Text = "Modifikátor:";

                // status bar
                if (comboBox2.SelectedIndex == 2)
                {
                    int val = (int)numericUpDown5.Value - word;
                    string round = "kol";

                    if (val < 0) val = 0;
                    if (val == 1) round = "kolo";
                    if (val > 1 && val < 5) round = "kola";

                    this.toolStripStatusLabel2.Text = "Zbývá: " + val + " " + round;
                }

                // konec hry
                string end = "";
                if (word == words.Count) end = "Databáze slov byla vyčerpána."; // vycerpani slov
                if (comboBox2.SelectedIndex == 1 && timecounter >= (int)numericUpDown5.Value)
                {
                    end = "Vypršel časový limit."; // casovy limit
                    timer2.Stop();
                } 
                if (comboBox2.SelectedIndex == 2 && word == (int)numericUpDown5.Value) end = "Byla odehrána všechna kola."; // pocet kol

                if (end != "")
                {
                    button4.Enabled = false;
                    button6.Enabled = false;
                    label13.Enabled = false;
                    label14.Enabled = false;
                    numericUpDown4.Enabled = false;
                    button1.Text = "Nová hra";
                    label6.Text = "???";
                    
                    // vitez
                    string winner = "";
                    if (score[0] == score.Max()) winner += radioButton1.Text + ", ";
                    if (score[1] == score.Max()) winner += radioButton2.Text + ", ";
                    if (score[2] == score.Max()) winner += radioButton3.Text + ", ";
                    if (score[3] == score.Max()) winner += radioButton4.Text + ", ";

                    MessageBox.Show("Konec hry! " + end + "\nVítězem se stal " + winner + "gratulujeme!", "Konec hry", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string str = "Máš na to! verze 1.0\n\nCopyright © 2009 Petr Nohejl\n\nVěnováno Míšovi. Program je freeware.";
            MessageBox.Show(str, "O programu...", MessageBoxButtons.OK, MessageBoxIcon.Information);            
        }

        private void button6_Click(object sender, EventArgs e)
        {
            string playerName = "";
            int playerScore = 0;
            int player = 0;

            if (radioButton1.Checked)
            {
                player = 0;
                playerName = radioButton1.Text;
            }
            else if (radioButton2.Checked)
            {
                player = 1;
                playerName = radioButton2.Text;
            }
            else if (radioButton3.Checked)
            {
                player = 2;
                playerName = radioButton3.Text;
            }
            else if (radioButton4.Checked)
            {
                player = 3;
                playerName = radioButton4.Text;
            }
            playerScore = score[player];

            InputBoxResult result = InputBox.Show(playerName, "Upravit score", playerScore.ToString(), new InputBoxValidatingHandler(inputBox_Validating));
            if (result.OK)
            {
                score[player] = System.Convert.ToInt32(result.Text);
                label7.Text = score[0] + " bodů";
                label8.Text = score[1] + " bodů";
                label9.Text = score[2] + " bodů";
                label10.Text = score[3] + " bodů";
            }

            
        }

        private void inputBox_Validating(object sender, InputBoxValidatingArgs e)
        {
            // retezec neni cislo
            try
            {
                System.Convert.ToInt32(e.Text);
            }
            catch
            {
                e.Cancel = true;
                e.Message = "NAN";
            }

            // prazdny retezec
            if (e.Text.Trim().Length == 0)
            {
                e.Cancel = true;
                e.Message = "Required";
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.comboBox2.SelectedIndex == 0)
            {
                numericUpDown5.Visible = false;
                label17.Visible = false;
            }
            else
            {
                numericUpDown5.Visible = true;
                label17.Visible = true;

                if (this.comboBox2.SelectedIndex == 1)
                {
                    label17.Text = "minut";
                }
                else if (this.comboBox2.SelectedIndex == 2)
                    label17.Text = "kol";
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            timecounter++;

            int val = (int)numericUpDown5.Value - timecounter;
            string min = "minut";

            if (val < 0) val = 0;
            if (val == 1) min = "minuta";
            if (val > 1 && val < 5) min = "minuty";

            this.toolStripStatusLabel2.Text = "Zbývá: " + val + " " + min;
        }
    }
}

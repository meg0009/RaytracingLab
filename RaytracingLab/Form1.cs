﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RaytracingLab {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        View view;

        private void Form1_Load(object sender, EventArgs e) {
            view = new View();
            view.setupView(glControl1.Width, glControl1.Height);
            //glControl1.Invalidate();
            Application.Idle += Application_Idle;
            //glControl1.Invalidate();
            int N = view.Materials.Length;
            for(int i = 0; i < N; i++) {
                listBox1.Items.Add(i);
            }
            listBox1.SelectedIndex = 0;
            textDepth.Text = Convert.ToString(view.maxDepth);
            int T = view.Triangles.Length;
            for(int i = 0; i < T; i++) {
                listBox2.Items.Add("Триугольник " + i);
            }
            int S = view.Spheres.Length;
            for(int i = 0; i < S; i++) {
                listBox2.Items.Add("Сфера " + i);
            }
            int C = view.Cubes.Length;
            for(int i =0; i < C; i++) {
                listBox2.Items.Add("Куб " + i);
            }
            listBox2.SelectedIndex = 0;
        }

        private void Application_Idle(object sender, EventArgs e) {
            while (glControl1.IsIdle) {
                //displayFPS();
                glControl1.Invalidate();
            }
        }

        private void glControl1_Paint(object sender, PaintEventArgs e) {
            view.Draw();
            glControl1.SwapBuffers();
        }

        private void Form1_Resize(object sender, EventArgs e) {
            groupBox3.Height = Height - 63;
            groupBox3.Location = new Point(Width - groupBox3.Width - 28, Height - groupBox3.Height - 51);
            groupBox2.Height = Height - 63;
            groupBox2.Location = new Point(groupBox3.Location.X - groupBox2.Width - 6, Height - groupBox2.Height - 51);
            groupBox1.Height = Height - 63;
            groupBox1.Location = new Point(groupBox2.Location.X - groupBox1.Width - 6, Height - groupBox1.Height - 51);
            glControl1.Height = Height - 63;
            glControl1.Width = groupBox1.Location.X - 18;
            view.Resize(glControl1.Width, glControl1.Height);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) {
            Material material = view.Materials[(int)listBox1.SelectedItem];
            materialColor.Text = material.Color.X + " " + material.Color.Y + " " + material.Color.Z;
            LightCoeffs.Text = material.LightCoeffs.X + " " + material.LightCoeffs.Y + " " + material.LightCoeffs.Z + " " + material.LightCoeffs.W;
            ReflectionCoef.Text = Convert.ToString(material.ReflectionCoef);
            RefractionCoef.Text = Convert.ToString(material.RefractionCoef);
            MaterialType.Text = Convert.ToString(material.MaterilaType);
        }

        private void button1_Click(object sender, EventArgs e) {
            float[] tmp = materialColor.Text.Trim().Split().Select((s) => Convert.ToSingle(s)).ToArray();
            float[] tmp2 = LightCoeffs.Text.Trim().Split().Select((s) => Convert.ToSingle(s)).ToArray();
            Material material = new Material(
                new OpenTK.Vector3(tmp[0], tmp[1], tmp[2]), 
                new OpenTK.Vector4(tmp2[0], tmp2[1], tmp2[2], tmp2[3]),
                Convert.ToSingle(ReflectionCoef.Text.Trim()),
                Convert.ToSingle(RefractionCoef.Text.Trim()),
                Convert.ToInt32(MaterialType.Text.Trim())
            );
            view.setMaterial((int)listBox1.SelectedItem, material);
        }

        private void button2_Click(object sender, EventArgs e) {
            view.maxDepth = Convert.ToInt32(textDepth.Text);
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e) {
            string str = (string)listBox2.SelectedItem;
            string[] arr = str.Trim().Split();
            int N = Convert.ToInt32(arr[1]);
            switch (arr[0]) {
                case "Триугольник":
                    textBox1.Text = Convert.ToString(view.Triangles[N].MaterialIdx);
                    break;
                case "Сфера":
                    textBox1.Text = Convert.ToString(view.Spheres[N].MaterialIdx);
                    break;
                case "Куб":
                    textBox1.Text = Convert.ToString(view.Cubes[N].MaterialIdx);
                    break;
            }
        }

        private void button3_Click(object sender, EventArgs e) {
            string str = (string)listBox2.SelectedItem;
            string[] arr = str.Trim().Split();
            int N = Convert.ToInt32(arr[1]);
            switch (arr[0]) {
                case "Триугольник":
                    view.setTriangleMaterial(N, Convert.ToInt32(textBox1.Text));
                    break;
                case "Сфера":
                    view.setSphereMaterial(N, Convert.ToInt32(textBox1.Text));
                    break;
                case "Куб":
                    view.setCubeMaterial(N, Convert.ToInt32(textBox1.Text));
                    break;
            }
        }
    }
}

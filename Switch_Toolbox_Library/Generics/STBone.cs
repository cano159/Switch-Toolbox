﻿using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Switch_Toolbox.Library
{
    public class STBone : TreeNodeCustom
    {
        private bool visbile = true;
        public bool Visible
        {
            get
            {
                return visbile;
            }
            set
            {
                visbile = value;
            }
        }

        public bool UseSegmentScaleCompensate;

        public STSkeleton skeletonParent;
        public BoneRotationType RotationType;
        public ushort BillboardIndex;
        public short RigidMatrixIndex;
        public short SmoothMatrixIndex;

        public float[] position = new float[] { 0, 0, 0 };
        public float[] rotation = new float[] { 0, 0, 0 };
        public float[] scale = new float[] { 1, 1, 1 };

        public Vector3 pos = Vector3.Zero, sca = new Vector3(1f, 1f, 1f);
        public Quaternion rot = Quaternion.FromMatrix(Matrix3.Zero);
        public Matrix4 Transform, invert;

        public Vector3 GetPosition()
        {
            return pos;
        }

        public Quaternion GetRotation()
        {
            return rot;
        }

        public Vector3 GetScale()
        {
            return sca;
        }

        public int GetIndex()
        {
            if (skeletonParent != null)
                return skeletonParent.bones.IndexOf(this);
            else
                return -1;
        }

        public void ConvertToQuaternion()
        {
            if (RotationType == BoneRotationType.Quaternion)
                return;

        }

        public void ConvertToEular()
        {
            if (RotationType == BoneRotationType.Euler)
                return;


        }

        public override void OnClick(TreeView treeView)
        {

        }

        public enum BoneRotationType
        {
            Euler,
            Quaternion,
        }

        public int parentIndex
        {
            set
            {
                if (Parent != null) Parent.Nodes.Remove(this);
                if (value > -1 && value < skeletonParent.bones.Count)
                {
                    skeletonParent.bones[value].Nodes.Add(this);
                }
            }

            get
            {
                if (Parent == null || !(Parent is STBone))
                    return -1;


                return skeletonParent.bones.IndexOf((STBone)Parent);
            }
        }

        public List<STBone> GetChildren()
        {
            List<STBone> l = new List<STBone>();
            foreach (STBone b in skeletonParent.bones)
                if (b.Parent == this)
                    l.Add(b);
            return l;
        }

        public STBone(STSkeleton skl)
        {
            skeletonParent = skl;
            ImageKey = "bone";
            SelectedImageKey = "bone";

            Checked = true;
        }

        public STBone()
        {
            ImageKey = "bone";
            SelectedImageKey = "bone";
        }

        public Matrix4 CalculateSmoothMatrix()
        {
            Matrix4 mat4 = new Matrix4();

            return Transform * invert;
        }
        public Matrix4 CalculateRigidMatrix()
        {
            Matrix4 mat4 = new Matrix4();


            return mat4;
        }

        public void Render()
        {
            if (!Runtime.OpenTKInitialized || !Runtime.renderBones)
                return;

            Vector3 pos_c = Vector3.TransformPosition(Vector3.Zero, Transform);

            if (IsSelected)
            {
                GL.Color3(Color.Red);
            }
            else
                GL.Color3(Color.GreenYellow);

            RenderTools.DrawCube(pos_c, 0.1f);

            // now draw line between parent
            GL.Color3(Color.LightBlue);
            GL.LineWidth(2f);

            GL.Begin(PrimitiveType.Lines);
            if (Parent != null && Parent is STBone)
            {
                Vector3 pos_p = Vector3.TransformPosition(Vector3.Zero, ((STBone)Parent).Transform);
                GL.Vertex3(pos_c);
                GL.Color3(Color.Blue);
                GL.Vertex3(pos_p);
            }
            GL.End();
        }
    }
}

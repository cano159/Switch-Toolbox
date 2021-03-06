﻿using System;
using System.Windows.Forms;
using Switch_Toolbox.Library;
using Switch_Toolbox.Library.Animations;
using Switch_Toolbox.Library.NodeWrappers;
using FirstPlugin;
using Syroot.NintenTools.NSW.Bfres;
using ResU = Syroot.NintenTools.Bfres;

namespace Bfres.Structs
{
    public class FSHA : Animation
    {
        public ShapeAnim ShapeAnim;
        public ResU.ShapeAnim ShapeAnimU;

        public FSHA()
        {
            ImageKey = "shapeAnimation";
            SelectedImageKey = "shapeAnimation";
        }

        public class ShapeAnimEntry : STGenericWrapper
        {
            public VertexShapeAnim vertexShapeAnim;
            public ResU.VertexShapeAnim vertexShapeAnimU;

            public ShapeAnimEntry()
            {

            }

            public void LoadAnim(VertexShapeAnim vertexAnim)
            {
                vertexShapeAnim = vertexAnim;
            }

            public void LoadAnim(ResU.VertexShapeAnim vertexAnim)
            {
                vertexShapeAnimU = vertexAnim;
            }

            public VertexShapeAnim SaveData()
            {
                vertexShapeAnim.Name = Text;
                return vertexShapeAnim;
            }

            public ResU.VertexShapeAnim SaveDataU()
            {
                vertexShapeAnimU.Name = Text;
                return vertexShapeAnimU;
            }
        }

        public ResFile GetResFile() {
            return ((BFRESGroupNode)Parent).GetResFile();
        }

        public ResU.ResFile GetResFileU() {
            return ((BFRESGroupNode)Parent).GetResFileU();
        }

        public override void OnClick(TreeView treeView) => UpdateEditor();

        public void UpdateEditor(){
            ((BFRES)Parent.Parent.Parent).LoadEditors(this);
        }

        public FSHA(ResU.ShapeAnim anim) { LoadAnim(anim); }
        public FSHA(ShapeAnim anim) { LoadAnim(anim); }

        private void LoadAnim(ShapeAnim shapeAnim)
        {
            Text = shapeAnim.Name;

            ShapeAnim = shapeAnim;
        }
        private void LoadAnim(ResU.ShapeAnim shapeAnim)
        {
            Text = shapeAnim.Name;

            ShapeAnimU = shapeAnim;
        }

        public override string ExportFilter => FileFilters.GetFilter(typeof(FSHA));

        public override void Export(string FileName)
        {
            ShapeAnim.Export(FileName, GetResFile());
        }

        public override void Replace(string FileName)
        {
            Replace(FileName, GetResFile(), GetResFileU());
        }

        public void Replace(string FileName, ResFile resFileNX, ResU.ResFile resFileU)
        {
            if (resFileNX != null)
            {
                ShapeAnim.Import(FileName);
                ShapeAnim.Name = Text;
            }
            else
            {
                ShapeAnimU.Import(FileName, resFileU);
                ShapeAnimU.Name = Text;
            }
        }
    }
}

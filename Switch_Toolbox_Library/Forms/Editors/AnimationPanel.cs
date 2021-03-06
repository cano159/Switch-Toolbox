﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using Switch_Toolbox.Library.Forms;
using Switch_Toolbox.Library.Animations;

namespace Switch_Toolbox.Library
{
    //Thanks to forge! Based on
    // https://github.com/jam1garner/Smash-Forge/blob/52844da94c7bed830d841e0d7e5d49c3f2c69471/Smash%20Forge/GUI/ModelViewport.cs

    public partial class AnimationPanel : STUserControl
    {
        public PlayerState AnimationPlayerState = PlayerState.Stop;

        public enum PlayerState
        {
            Playing,
            Pause,
            Stop,
        }

        public bool IsLooping
        {
            get { return loopChkBox.Checked; }
            set { loopChkBox.Checked = value; }
        }

        public bool IsPlaying
        {
            get
            {
                return AnimationPlayerState == PlayerState.Playing;
            }
        }

        private static AnimationPanel _instance;
        public static AnimationPanel Instance { get { return _instance == null ? _instance = new AnimationPanel() : _instance; } }

        //Animation Functions
        public int AnimationSpeed = 60;
        public float Frame = 0;

        // Frame rate control
        private Thread renderThread;
        private bool renderThreadIsUpdating = false;
        public bool isOpen = true;

        private Animation currentAnimation;
        public Animation CurrentAnimation
        {
            get
            {
                return currentAnimation;
            }
            set
            {
                if (value == null)
                    return;

                int frameCount = 1;

                if (value.FrameCount != 0)
                    frameCount = value.FrameCount;

                ResetModels();
                currentAnimation = value;
                totalFrame.Maximum = frameCount;
                totalFrame.Value = frameCount;
                currentFrameUpDown.Maximum = frameCount;
                animationTrackBar.TickDivide = 1;
                animationTrackBar.Maximum = frameCount;
                animationTrackBar.Minimum = 0;
                currentFrameUpDown.Value = 1;
                currentFrameUpDown.Value = 0;
            }
        }

        public void ResetModels()
        {
            var viewport = LibraryGUI.Instance.GetActiveViewport();
            if (viewport == null)
                return;
            if (viewport.scene == null)
                return;

            foreach (var drawable in viewport.scene.objects)
            {
                if (drawable is STSkeleton)
                {
                    ((STSkeleton)drawable).reset();
                }
            }
        }

        public AnimationPanel()
        {
            InitializeComponent();

            BackColor = FormThemes.BaseTheme.FormBackColor;
            ForeColor = FormThemes.BaseTheme.FormForeColor;

            animationTrackBar.BackColor = FormThemes.BaseTheme.FormBackColor;
            animationTrackBar.ForeColor = FormThemes.BaseTheme.FormForeColor;

            animationTrackBar.ThumbInnerColor = FormThemes.BaseTheme.TimelineThumbColor;
            animationTrackBar.ThumbOuterColor = FormThemes.BaseTheme.TimelineThumbColor;

            this.animationTrackBar.BarInnerColor = FormThemes.BaseTheme.FormBackColor;
            this.animationTrackBar.BarPenColorBottom = FormThemes.BaseTheme.FormBackColor;
            this.animationTrackBar.BarPenColorTop = FormThemes.BaseTheme.FormBackColor;
            this.animationTrackBar.ElapsedInnerColor = FormThemes.BaseTheme.FormBackColor;
            this.animationTrackBar.ElapsedPenColorBottom = FormThemes.BaseTheme.FormBackColor;
            this.animationTrackBar.ElapsedPenColorTop = FormThemes.BaseTheme.FormBackColor;

            panel1.BackColor = FormThemes.BaseTheme.FormBackColor;
            animationPlayBtn.BackColor = FormThemes.BaseTheme.FormBackColor;
            button2.BackColor = FormThemes.BaseTheme.FormBackColor;
            animationPlayBtn.ForeColor = FormThemes.BaseTheme.FormForeColor;
            button2.ForeColor = FormThemes.BaseTheme.FormForeColor;

            totalFrame.ForeColor = FormThemes.BaseTheme.FormForeColor;
            totalFrame.BackColor = FormThemes.BaseTheme.FormBackColor;
            currentFrameUpDown.ForeColor = FormThemes.BaseTheme.FormForeColor;
            currentFrameUpDown.BackColor = FormThemes.BaseTheme.FormBackColor;

            this.LostFocus += new System.EventHandler(AnimationPanel_LostFocus);
        }

        private void Play()
        {
            AnimationPlayerState = PlayerState.Playing;
            UpdateAnimationUI();
        }

        private void Pause()
        {
            AnimationPlayerState = PlayerState.Stop;
            UpdateAnimationUI();
        }

        private void Stop()
        {
            currentFrameUpDown.Value = 0;
            AnimationPlayerState = PlayerState.Stop;
            UpdateAnimationUI();
        }

        private void UpdateAnimationUI()
        {
            animationPlayBtn.BackgroundImage = IsPlaying ? Properties.Resources.stop
                : Properties.Resources.arrowR;
        }

        private void UpdateAnimationFrame()
        {
            if (IsPlaying)
            {
                if (currentFrameUpDown.InvokeRequired)
                {
                    currentFrameUpDown.BeginInvoke((Action)(() =>
                    {
                        AdvanceNextFrame();
                    }));
                }
                else
                {
                    AdvanceNextFrame();
                }
            }
        }

        private void AdvanceNextFrame()
        {
            if (animationTrackBar.Value == animationTrackBar.Maximum)
            {
                if (IsLooping)
                    currentFrameUpDown.Value = 0;
                else
                    Stop();
            }
            else
            {
                currentFrameUpDown.Value++;
            }

            currentFrameUpDown.Refresh();
            totalFrame.Refresh();
        }

        private void animationPlayBtn_Click(object sender, EventArgs e)
        {
            if (AnimationPlayerState == PlayerState.Playing)
                Pause();
            else
                Play();
        }

        private void totalFrame_ValueChanged(object sender, EventArgs e)
        {
            if (currentAnimation == null) return;
            if (totalFrame.Value < 1)
            {
                totalFrame.Value = 1;
            }
            else
            {
                if (currentAnimation.Tag is Animation)
                    ((Animation)currentAnimation.Tag).FrameCount = (int)totalFrame.Value;
                currentAnimation.FrameCount = (int)totalFrame.Value;
                animationTrackBar.Value = 0;
                animationTrackBar.Maximum = currentAnimation.FrameCount;
                animationTrackBar.Minimum = 0;
            }
        }
        private void UpdateViewport()
        {
            if (IsDisposed)
                return;

            Viewport viewport = LibraryGUI.Instance.GetActiveViewport();

            if (viewport == null)
                return;

            if (viewport.GL_ControlLegacy != null &&
                !viewport.GL_ControlLegacy.IsDisposed)
            {
                if (viewport.GL_ControlLegacy.InvokeRequired)
                {
                    viewport.GL_ControlLegacy.Invoke((MethodInvoker)delegate {
                        // Running on the UI thread
                        viewport.GL_ControlLegacy.Invalidate();
                    });
                }
                else
                {
                    viewport.GL_ControlLegacy.Invalidate();
                }
            }
            else
            {
                if (viewport.GL_ControlModern == null || viewport.GL_ControlModern.IsDisposed || viewport.GL_ControlModern.Disposing)
                    return;

                if (viewport.GL_ControlModern.InvokeRequired)
                {
                    viewport.GL_ControlModern.Invoke((MethodInvoker)delegate {
                        // Running on the UI thread
                        viewport.GL_ControlModern.Invalidate();
                    });
                }
                else
                {
                    viewport.GL_ControlModern.Invalidate();
                }
            }
        }

        private void nextButton_Click(object sender, EventArgs e) {
            if (animationTrackBar.Value < animationTrackBar.Maximum)
                animationTrackBar.Value++;
        }
        private void prevButton_Click(object sender, EventArgs e) {
              if (animationTrackBar.Value > 0)
                animationTrackBar.Value--;
        }

        private void animationTrackBar_Scroll(object sender, EventArgs e)
        {

        }

        private void animationTrackBar_ValueChanged(object sender, EventArgs e)
        {
            currentFrameUpDown.Value = animationTrackBar.Value;
            UpdateViewport();
            SetAnimationsToFrame(animationTrackBar.Value);

            if (!renderThreadIsUpdating || !IsPlaying)
                UpdateViewport();
        }
        private void SetAnimationsToFrame(int frameNum)
        {
            if (currentAnimation == null)
                return;

            var viewport = LibraryGUI.Instance.GetActiveViewport();
            if (viewport == null || viewport.scene == null)
                return;

            var anim = currentAnimation.Tag;

            float animFrameNum = frameNum;


            if (anim is MaterialAnimation)
            {
                ((MaterialAnimation)anim).SetFrame(animFrameNum);
                ((MaterialAnimation)anim).NextFrame(viewport);
            }
            else if (anim is VisibilityAnimation)
            {
                ((VisibilityAnimation)anim).SetFrame(animFrameNum);
                ((VisibilityAnimation)anim).NextFrame(viewport);
            }
            else if (anim is CameraAnimation)
            {
                ((CameraAnimation)anim).SetFrame(animFrameNum);
                ((CameraAnimation)anim).NextFrame(viewport);
            }
            else if (anim is LightAnimation)
            {
                ((LightAnimation)anim).SetFrame(animFrameNum);
                ((LightAnimation)anim).NextFrame(viewport);
            }
            else if (anim is FogAnimation)
            {
                ((FogAnimation)anim).SetFrame(animFrameNum);
                ((FogAnimation)anim).NextFrame(viewport);
            }
            else //Play a skeletal animation if it's not the other types
            {
                foreach (var drawable in viewport.scene.objects)
                {
                    if (drawable is STSkeleton)
                    {
                        currentAnimation.SetFrame(animFrameNum);
                        currentAnimation.NextFrame((STSkeleton)drawable);
                    }
                }
            }


            //Add frames to the playing animation
            currentAnimation.Frame += 1f;

            //Reset it when it reaches the total frame count
            if (currentAnimation.Frame >= currentAnimation.FrameCount)
            {
              currentAnimation.Frame = 0;
            }
        }

        private void currentFrameUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (currentFrameUpDown.Value > totalFrame.Value)
                currentFrameUpDown.Value = totalFrame.Value;

            animationTrackBar.Value = (int)currentFrameUpDown.Value;

            animationTrackBar.Refresh();
        }

        public void AnimationPanel_FormClosed()
        {
            isOpen = false;
            Dispose();
        }



        private void AnimationPanel_Load(object sender, EventArgs e)
        {
            Viewport viewport = LibraryGUI.Instance.GetActiveViewport();
            if (viewport != null)
            {
                if (viewport.GL_ControlLegacy != null)
                    viewport.GL_ControlLegacy.VSync = Runtime.enableVSync;
                else
                    viewport.GL_ControlModern.VSync = Runtime.enableVSync;
            }

            renderThread = new Thread(new ThreadStart(RenderAndAnimationLoop));
            renderThread.Start();
        }

        private void RenderAndAnimationLoop()
        {
            if (IsDisposed)
                return;
            // TODO: We don't really need two timers.
            Stopwatch animationStopwatch = Stopwatch.StartNew();


            // Wait for UI to load before triggering paint events.
            //  int waitTimeMs = 500;
            //  Thread.Sleep(waitTimeMs);

           // UpdateViewport();

            int frameUpdateInterval = 5;
            int animationUpdateInterval = 16;

            while (isOpen)
            {
                // Always refresh the viewport when animations are playing.
                if (renderThreadIsUpdating || IsPlaying)
                {
                    if (animationStopwatch.ElapsedMilliseconds > animationUpdateInterval)
                    {
                        UpdateAnimationFrame();
                        animationStopwatch.Restart();
                    }
                }
                else
                {
                    // Avoid wasting the CPU if we don't need to render anything.
                    Thread.Sleep(1);
                }
            }
        }

        private void AnimationPanel_Enter(object sender, EventArgs e)
        {
        }

        private void AnimationPanel_LostFocus(object sender, EventArgs e)
        {
            renderThreadIsUpdating = false;
        }

        private void AnimationPanel_Click(object sender, EventArgs e)
        {
            renderThreadIsUpdating = true;
        }

        private void AnimationPanel_Leave(object sender, EventArgs e)
        {
            renderThreadIsUpdating = false;
        }
        public void ClosePanel()
        {
            ResetModels();

            renderThread.Abort();
            renderThreadIsUpdating = false;
            currentAnimation = null;
            isOpen = false;
            Dispose();

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void colorSlider1_Scroll(object sender, ScrollEventArgs e)
        {

        }
    }
}

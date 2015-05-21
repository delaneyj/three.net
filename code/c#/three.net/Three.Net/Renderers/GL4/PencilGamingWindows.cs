using Pencil.Gaming;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Three.Net.Math;
using Three.Net.Utils;

namespace Three.Net.Renderers.GL4
{
    public class PencilKeyInfo
    {
        public readonly Key Key;
        public readonly int Scancode;
        public readonly KeyAction Action;
        public readonly KeyModifiers Modifiers;

        public PencilKeyInfo(Pencil.Gaming.Key key, int scancode, KeyAction action, KeyModifiers modifiers)
        {
            Key = key;
            Scancode = scancode;
            Action = action;
            Modifiers = modifiers;
        }
    }

    public class PencilGamingWindows
    {
        private int width, height;
        private string title;
        private bool isFullscreen, shouldVsync, firstTime;
        private GlfwWindowPtr window;

        public int Width { get { return width; } set { width = value; CreateWindow(); } }
        public int Height { get { return height; } set { height = value; CreateWindow(); } }
        public float AspectRatio { get { return width / (float)height; } }
        public string Title { get { return title; } set { title = value; CreateWindow(); } }
        public bool IsFullScreen { get { return isFullscreen; } set { isFullscreen = value; CreateWindow(); } }
        public Vector2 MousePosition
        {
            get
            {
                double x, y;
                Glfw.GetCursorPos(window, out x, out y);
                return new Vector2((float)x, (float)y);
            }
        }
        public Subscribable<PencilGamingWindows> OnExit;
        private Trigger<PencilGamingWindows> onExit;

        public Subscribable<PencilGamingWindows> OnWindowOpened;
        private Trigger<PencilGamingWindows> onWindowOpened;

        public Subscribable<PencilGamingWindows> OnWindowClosed;
        private Trigger<PencilGamingWindows> onWindowClosed;

        public Subscribable<PencilGamingWindows> OnWindowResized;
        private Trigger<PencilGamingWindows> onWindowResized;

        public Subscribable<PencilKeyInfo> OnKeyboard;
        private Trigger<PencilKeyInfo> onKeyboard;

        private bool rebuiltWindow = false;

        public PencilGamingWindows(int width, int height, string title, bool isFullscreen, bool shouldVsync)
        {
            onWindowOpened = new Trigger<PencilGamingWindows>();
            OnWindowOpened = onWindowOpened;

            onWindowClosed = new Trigger<PencilGamingWindows>();
            OnWindowClosed = onWindowClosed;

            onWindowResized = new Trigger<PencilGamingWindows>();
            OnWindowResized = onWindowResized;

            onKeyboard = new Trigger<PencilKeyInfo>();
            OnKeyboard = onKeyboard;

            onExit = new Trigger<PencilGamingWindows>();
            OnExit = onExit;

            window = GlfwWindowPtr.Null;
            this.width = width;
            this.height = height;
            this.title = title;
            this.isFullscreen = isFullscreen;
            this.shouldVsync = shouldVsync;
            this.firstTime = true;
            if (!Glfw.Init())
            {
                Console.Error.WriteLine("ERROR: Could not initialize GLFW, shutting down.");
                Environment.Exit(1);
            }
        }
        
        public void CreateWindow()
        {
            var monitor = isFullscreen ? Glfw.GetPrimaryMonitor() : GlfwMonitorPtr.Null;
            var currentWindow = Glfw.CreateWindow(Width, Height, Title, monitor, window);

            Glfw.SetKeyCallback(currentWindow, new GlfwKeyFun((w, key, scancode, action, modifiers) =>
            {
                var info = new PencilKeyInfo(key, scancode, action, modifiers);
                onKeyboard.Fire(info);
            }));

            Glfw.SetWindowCloseCallback(currentWindow, new GlfwWindowCloseFun(w =>
            {
                Exit();
            }));

            Glfw.SetWindowSizeCallback(currentWindow, new GlfwWindowSizeFun((w, width, height) =>
            {
                this.width = width;
                this.height = height;
                onWindowResized.Fire(this);
            }));

            Glfw.SwapInterval(shouldVsync ? 1 : 0);
            Glfw.MakeContextCurrent(currentWindow);
            if(!firstTime) Glfw.SetWindowShouldClose(window, true);

            window = currentWindow;
            rebuiltWindow = true;
            onWindowOpened.Fire(this);

            firstTime = false;
        }

        private void CloseWindow()
        {
            if (window.Equals(GlfwWindowPtr.Null)) return;
            Glfw.SetWindowShouldClose(window,true);
            onWindowClosed.Fire(this);
        }

        internal void MakeCurrent()
        {
            Glfw.MakeContextCurrent(window);
        }

        internal bool ProcessEvents()
        {
            Glfw.PollEvents();
            if(rebuiltWindow)
            {
                rebuiltWindow = false;
                return true;
            }
            return false;
        }

        internal void SwapBuffers()
        {
            Glfw.SwapBuffers(window);
        }

        public void Exit()
        {
            CloseWindow();
            onExit.Fire(this);
        }
    }
}

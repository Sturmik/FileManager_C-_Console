using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimpleFileManager
{
    // Basic Form
    interface Form
    {
        int X { get; set; }
        int Y { get; set; }
        int Width { get; set; }
        int Height { get; set; }
    }

    /// <summary>
    /// Class for forming windows without text
    /// </summary>
    internal class Window : Form
    {
        #region Variables/Properties

        // X and y pos of the form
        private int x, y;
        // Height and width of the form
        private int width, height;
        // Color of the button
        private ConsoleColor outterCol;

        public int X
        {
            get { return x; }
            set { if (x >= 0) x = value; }
        }
        public int Y
        {
            get { return y; }
            set { if (y >= 0) y = value; }
        }
        public int Width
        {
            get { return width; }
            set { if (width >= 0) width = value; }
        }
        public int Height
        {
            get { return height; }
            set { if (height >= 0) height = value; }
        }
        public ConsoleColor OutterCol
        {
            get { return outterCol; }
            set { outterCol = value; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor of the button
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="outterCol">Outter color</param>
        /// <param name="innerCol">Inner color</param>
        public Window(int x, int y, int width, int height, ConsoleColor outterCol)
        {
            if (x < 0)
                x = 0;
            if (y < 0)
                y = 0;
            if (height < 0)
                height = 1;
            if (width < 0)
                width = 1;

            X = x;
            Y = y;
            Height = height;
            Width = width;
            OutterCol = outterCol;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows window
        /// </summary>
        public void ShowWindow()
        {
            Console.SetCursorPosition(x, y);

            int tempX = x;
            int tempY = y;

            int spaceLeft;


            Console.ForegroundColor = outterCol;
            for (int i = 0; i < width; i++)
            {
                Console.Write("-");
            }
            for (int i = 0; i < height; i++)
            {
                spaceLeft = width - 2;

                Console.SetCursorPosition(tempX, ++tempY);

                Console.Write("|");

                for (int s = 0; s < spaceLeft; s++)
                    Console.Write(" ");


                Console.Write("|");
            }
            Console.SetCursorPosition(tempX, ++tempY);
            for (int i = 0; i < width; i++)
            {
                Console.Write("-");
            }

            Console.ResetColor();
        }

        #endregion
    }

    /// <summary>
    /// Class button is made for displaying information or creating buttons to press
    /// </summary>
    public class Button : Form
    {
        #region Variables/Properties

        // X and y pos of the form
        private int x, y;
        // Height and width of the form
        private int width, height;
        // Text, which will be inside of the button
        private List<string> innerText;
        // Text, which will be showed in the button
        private List<string> showingText;
        // Color of the button
        private ConsoleColor outterCol, innerCol;
        // The color of the button, when you will press it or choose it
        private static ConsoleColor chosenColorOut, chosenColorIn, chosenColorBack;
        // Checks if the button is pressed
        bool pressed;

        public int X 
        { 
            get { return x; } 
            set { if (x >= 0) x = value; }
        }
        public int Y
        { 
            get { return y; }
            set { if (y >= 0) y = value; }
        }
        public int Width
        {
            get { return width; }
            set { if (width >= 0) width = value; }
        }
        public int Height 
        {
            get { return height; } 
            set { if (height >= 0) height = value; } 
        }
        public List<string> InnerText
        {
            get { return innerText; } 
        }
        public ConsoleColor OutterCol
        { 
            get { return outterCol; }
            set { outterCol = value; }
        }
        public ConsoleColor InnerCol
        {
            get { return innerCol; }
            set { innerCol = value; }
        }
        public ConsoleColor ChosenColorOut 
        {
            get { return chosenColorOut; }
            set { chosenColorOut = value; }
        }
        public ConsoleColor ChosenColorIn
        {
            get { return chosenColorIn; }
            set { chosenColorIn = value; }
        }
        public ConsoleColor ChosenColorBack
        {
            get { return chosenColorBack; }
            set { chosenColorBack = value; }
        }
        public bool Pressed
        {
            get { return pressed; }
            set { pressed = value; }
        }


        #endregion

        #region Constructor

        static Button()
        {
            chosenColorIn = ConsoleColor.Black;
            chosenColorOut = ConsoleColor.Red;
            chosenColorBack = ConsoleColor.White;
        }

        /// <summary>
        /// Constructor of the button
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="innerText">Inner text</param>
        /// <param name="outterCol">Outter color</param>
        /// <param name="cutBeg_cutEnd">True - cuts string from begin to fit it in button | False starts from the end</param>
        /// <param name="innerCol">Inner color</param>
        public Button(int x, int y, int width, int height, ConsoleColor outterCol, ConsoleColor innerCol, bool cutBeg_cutEnd = false, params string[] innerText)
        {
            if (x < 0)
                x = 0;
            if (y < 0)
                y = 0;
            if (height < 0)
                height = 1;
            if (width < 0)
                width = 1;
            if (innerText.Length > height)
            {
                List<string> Fill = new List<string>();
                for (int i = 0; i < height; i++)
                {
                    Fill.Add(innerText[i]);
                }
                innerText = Fill.ToArray();
            } 

            X = x;
            Y = y;
            Height = height;
            Width = width;
            this.innerText = innerText.ToList();
            showingText = new List<string>();
            OutterCol = outterCol;
            InnerCol = innerCol;


            FitInString(cutBeg_cutEnd);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Cleans button from text inside
        /// </summary>
        public void ClearInnerText()
        {
            InnerText.Clear();
        }

        /// <summary>
        /// Function, which changes text inside of the form
        /// </summary>
        /// <param name="newText">New text</param>
        /// <param name="begin_End">True - cut from begining | False - cut from end</param>
        public void ChangeInnerText(bool begin_End = true, params string[] newText)
        {
            innerText = newText.ToList();
            FitInString(begin_End);
        }

        /// <summary>
        /// Helping function, which cuts the string if it doesn't fit in the button
        /// </summary>
        /// <param name="begin_end">True - cut start of the string | False - cut end of the string</param>
        private void FitInString(bool begin_end)
        {
            int checkCut;

            showingText.Clear();
            showingText.AddRange(innerText);

            for (int i = 0; i < showingText.Count; i++)
            {
                if (showingText[i].Length >= Width)
                {
                    checkCut = showingText[i].Length - Width + 2;

                    try
                    {
                        if (begin_end == true)
                            showingText[i] = showingText[i].Remove(0, checkCut);
                        else
                            showingText[i] = showingText[i].Remove(showingText[i].Length - checkCut - 1, checkCut);
                    }
                    catch (Exception e) {}
                }
            }
        }

        /// <summary>
        /// Shows button | Pressed in true condition will give color of pressed button |
        /// </summary>
        public void ShowButton()
        {
            Console.SetCursorPosition(x, y);

            int tempX = x;
            int tempY = y;

            int spaceLeft;

            ConsoleColor insideCol;
            ConsoleColor outterCol;
            ConsoleColor behindCol;

            if (pressed == true)
            {
                insideCol = ChosenColorIn;
                outterCol = ChosenColorOut;
                behindCol = ChosenColorBack;
            }
            else
            {
                insideCol = InnerCol;
                outterCol = OutterCol;
                behindCol = ConsoleColor.Black;
            }

            Console.ForegroundColor = outterCol;
            for (int i = 0; i < width; i++)
            {
                Console.Write("-");
            }
            for (int i = 0; i < height; i++)
            {
                spaceLeft = width - 2;

                Console.SetCursorPosition(tempX, ++tempY);

                Console.Write("|");
                Console.ForegroundColor = insideCol;
                Console.BackgroundColor = behindCol;

                if (InnerText.Count > i)
                {
                    Console.Write(showingText[i]);
                    spaceLeft -= showingText[i].Length;
                }

                for (int s = 0; s < spaceLeft; s++)
                    Console.Write(" ");

                Console.ResetColor();

                Console.ForegroundColor = outterCol;
                Console.Write("|");
            }
            Console.SetCursorPosition(tempX, ++tempY);
            Console.ForegroundColor = outterCol;
            for (int i = 0; i < width; i++)
            {
                Console.Write("-");
            }

            Console.ResetColor();
        }

        #endregion
    }
}

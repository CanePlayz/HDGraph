using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Globalization;

namespace HDGraphiqueurGUI
{
    public partial class WaitForm : Form
    {
        #region Constructeur
        /// <summary>
        /// Constructeur. Inutile dans la plupart des cas : utiliser les m�thodes statiques ShowWaitForm et HideWaitForm.
        /// </summary>
        public WaitForm()
        {
            InitializeComponent();
        }
        #endregion


        public void SetMessage(string message)
        {
            labelInformation.Text = message;
        }

        private static Thread myThread = null;

        // Ev�nement de signal de fin de thread
        private static AutoResetEvent _endThreadCalculsEvent = new AutoResetEvent(false);
        private static string formMessage = "";
        private static CultureInfo threadCulture = null;
        private static WaitForm myWaitForm;

        public static CultureInfo ThreadCulture
        {
            get { return WaitForm.threadCulture; }
            set { WaitForm.threadCulture = value; }
        }

        private static void LoadFormIfNecessary()
        {
            if (threadCulture != null)
                Thread.CurrentThread.CurrentUICulture = threadCulture;
            if (myWaitForm == null)
            {
                myWaitForm = new WaitForm();
                myWaitForm.labelInformation.Text = formMessage;
                myWaitForm.Show();
                //myWaitForm.CenterToParent();
                Application.DoEvents();
                while (!_endThreadCalculsEvent.WaitOne(50, false))
                {
                    if (myWaitForm.labelInformation.Text != formMessage)
                        myWaitForm.labelInformation.Text = formMessage;
                    Application.DoEvents();
                }
                myWaitForm.Close();
                myWaitForm = null;
            }
        }

        /// <summary>
        /// Affiche une fen�tre pour faire patienter l'utilisateur (dans un thread � part, pour ne pas bloquer l'ex�cution).
        /// </summary>
        /// <param name="parent">Non utilis� pour le moment.</param>
        /// <param name="message"></param>
        public static void ShowWaitForm(Form parent, string message)
        {

            formMessage = message;
            if (myThread == null || myThread.ThreadState == ThreadState.Stopped || myThread.ThreadState == ThreadState.Unstarted)
            {
                // LoadFormIfNecessary est la fonction ex�cut�e par le thread.
                myThread = new Thread(new ThreadStart(LoadFormIfNecessary));
                myThread.Start();
            }
        }

        /// <summary>
        /// Ferme la fen�tre d'attente affich�e avec la m�thode ShowWaitForm (si elle existe).
        /// </summary>
        public static void HideWaitForm()
        {
            if (myThread != null)
            {
                // L'evenement passe � l'�tat signal�
                _endThreadCalculsEvent.Set();
                // On attend la fin du thread.
                myThread.Join();
            }
        }
    }
}
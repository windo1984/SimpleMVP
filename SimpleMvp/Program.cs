﻿using System;
using System.Windows.Forms;
using Infrastructure.Nhibernate;
using NHibernate;
using SimpleMvp.Bases;
using SimpleMvp.Infrastructure;
using SimpleMvp.Infrastructure.Bases;
using StructureMap;

namespace SimpleMvp
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application. 
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var buildSessionFactory = new ConfigurationFactory().Build().BuildSessionFactory();

            ObjectFactory.Initialize(x =>
                                         {
                                             x.Scan(s =>
                                                        {
                                                            s.AssembliesFromApplicationBaseDirectory();
                                                            s.WithDefaultConventions();
                                                            s.ConnectImplementationsToTypesClosing(typeof (IPresenter<>));
                                                        });

                                             x.For(typeof (IPresenterFactory<>)).Use(typeof (PresenterFactory<>));

                                             x.For<ISession>().Use(buildSessionFactory.OpenSession);

                                             // Get internal Presenter Factory with Ctor Parameter
                                             x.For<Func<Type, object, IPresenter<IView>>>().Use(
                                                 (type, param) => (IPresenter<IView>) ObjectFactory
                                                                                          .With(param.GetType(), param)
                                                                                          .GetInstance(type));
                                             // Get internal Presenter Factory without Ctor Parameter
                                             x.For<Func<Type, IPresenter<IView>>>().Use(
                                                 type => (IPresenter<IView>) ObjectFactory.GetInstance(type));
                                         });

            using (var mainForm = ObjectFactory.GetInstance<IPresenter<IMainView>>())
            {
                Application.Run((Form) mainForm.CurrentView);
            }
        }
    }
}
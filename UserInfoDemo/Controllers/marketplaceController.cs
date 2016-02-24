﻿using System;
using System.Web.Mvc;
using ADPClient;
using ADPClient.Product;
using ADPClient.Product.dto;

namespace UserInfoDemo
{
    public class marketplaceController : Controller
    {
        // GET: marketplace
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult Authorize()
        {

            String authorizationurl = null;
            AuthorizationCodeConnection connection = null;

            // get new connection configuration
            // JSON config object placed in Web.config configuration or
            // set individual config object attributes
            String clientconfig = UserInfoDemo.Properties.Settings.Default.AuthorizationCodeConfiguration; //  AuthorizationCodeConfiguration;

            if (String.IsNullOrEmpty(clientconfig))
            {
                ViewBag.IsError = true;
                ViewBag.Message = "Settings file or default options not available.";
                Console.WriteLine(ViewBag.Message);
            }
            else {
                // Initialize the Connection Configuration Object.
                // specifying the ConnectionConfiguration type will get back the right ConnectionConfiguration type object.
                // AuthorizationCodeConfiguration object is returned
                // JavaScriptSerializer oJS = new JavaScriptSerializer();
                AuthorizationCodeConfiguration connectionCfg = JSONUtil.Deserialize<AuthorizationCodeConfiguration>(clientconfig);

                // create a new connection based on the connection configuration object provided
                connection = (AuthorizationCodeConnection)ADPApiConnectionFactory.createConnection(connectionCfg);

                try
                {
                    // Authorization Code Apps require a user to login to ADP
                    // So obtain the authorization URL to redirect the user's
                    // browser so they can login
                    authorizationurl = connection.getAuthorizationURL();

                    Console.WriteLine("Got auth URL... {0}... redirecting", authorizationurl);

                    // save connection for later use
                    HttpContext.Session["AuthorizationCodeConnection"] = connection;
                }
                catch (Exception e)
                {
                    ViewBag.isError = true;
                    ViewBag.Message = e.Message;
                    Console.WriteLine(ViewBag.Message);
                    return View("Index");
                }
            }

            return Redirect(authorizationurl);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult getUserInfo()
        {
            UserInfo userinfo = null;

            // get connection from session
            AuthorizationCodeConnection connection = HttpContext.Session["AuthorizationCodeConnection"] as AuthorizationCodeConnection;

            if (connection == null || ((AuthorizationCodeConfiguration)connection.connectionConfiguration).authorizationCode == null)
            {
                //is the connection available in session or is the 
                // cached connection expired then lets re-authorize
                return Authorize();
            }

            try
            {
                connection.connect();

                // connection was successfull 
                if (connection.isConnectedIndicator())
                {
                    // so get the worker like we wanted
                    UserInfoHelper helper = new UserInfoHelper(connection, null);
                    userinfo = helper.getUserInfo();
                }
            }
            catch (Exception e)
            {
                ViewBag.isError = true;
                ViewBag.Message = e.Message;
                Console.WriteLine(ViewBag.Message);
            }

            return View("Index", userinfo);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult Logout()
        {
            ViewBag.Message = "You're logged out.";
            HttpContext.Session["AuthorizationCodeConnection"] = null;

            return View("Index");
        }
    }
}

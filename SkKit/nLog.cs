using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;

namespace Common.log
{
    public class NLOG
    {

        private Logger logger;

        private NLOG(Logger _logger)
        {
            logger = _logger;
        }

        //cascade constructor so that the NLOG can be instantiated with the parameter "methodName" outside 
        //as well as can be instantiated with the parameter LoggerManager.GetCurrentClassLogger() within current class
        public NLOG(string methodName) : this(LogManager.GetLogger(methodName))
        {
        }

        //static constructor instantiate the Log 
        //before instantiating first class or reference any static member.
        public static NLOG Logger { get; private set; }
        static NLOG()
        {
            Logger = new NLOG(LogManager.GetCurrentClassLogger());
        }

        public void Trace(string message, params object[] args)
        {
            logger.Trace(message, args);
        }
        public string Getname()
        {
           return  logger.Name;
        }
        public void Trace(string message, Exception e, params object[] args)
        {
            logger.Trace(message, e, args);
        }

        public void Debug(string message, params object[] args)
        {
            logger.Debug(message, args);
        }

        public void Debug(string message, Exception e, params object[] args)
        {
            logger.Debug(message, e, args);
        }

        public void Info(string message, params object[] args)
        {
            logger.Info(message, args);
        }

        public void Info(string message, Exception e, params object[] args)
        {
            logger.Info(message, e, args);
        }

        public void Error(string message, params object[] args)
        {
            logger.Error(message, args);
        }

        public void Error(string message, Exception e, params object[] args)
        {
            logger.Error(message, e, args);
        }

        public void Fatal(string message, params object[] args)
        {
            logger.Fatal(message, args);
        }

        public void Fatal(string message, Exception e, params object[] args)
        {
            logger.Fatal(message, e, args);
        }

    }
}

using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;


namespace RealisticPopulationRevisited
{
    class Debugging
    {
        private static StringBuilder sb = new StringBuilder();
        private static Dictionary<String, int> messagesToSuppress = new Dictionary<string, int>();


        // Buffer warning
        public static void bufferWarning(string text)
        {
            sb.AppendLine("Realistic Population Revisited: " + text);
        }

        // Output buffer
        public static void releaseBuffer()
        {
            if (sb.Length > 0)
            {
                Debugging.Message(sb.ToString());
                sb.Remove(0, sb.Length);
            }
        }


        /// <summary>
        /// Prints a single-line debugging message to the Unity output log.
        /// </summary>
        /// <param name="message">Message to log</param>
        internal static void Message(string message)
        {
            Debug.Log(RealPopMod.ModName + ": " + message + ".");
        }


        /// <summary>
        /// Prints an exception message to the Unity output log.
        /// </summary>
        /// <param name="message">Message to log</param>
        internal static void LogException(Exception exception)
        {
            // Use StringBuilder for efficiency since we're doing a lot of manipulation here.
            StringBuilder message = new StringBuilder();

            message.AppendLine("caught exception!");
            message.AppendLine("Exception:");
            message.AppendLine(exception.Message);
            message.AppendLine(exception.Source);
            message.AppendLine(exception.StackTrace);

            // Log inner exception as well, if there is one.
            if (exception.InnerException != null)
            {
                message.AppendLine("Inner exception:");
                message.AppendLine(exception.InnerException.Message);
                message.AppendLine(exception.InnerException.Source);
                message.AppendLine(exception.InnerException.StackTrace);
            }

            // Write to log.
            Debugging.Message(message.ToString());
        }
    }
}

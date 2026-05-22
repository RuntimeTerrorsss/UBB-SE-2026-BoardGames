// <copyright file="ReceiptServiceConstants.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBoardGames.Data.Constants
{
    public class ReceiptServiceConstants
    {
        // graphical constants
        public const double HorizontalMargin = 40;
        public const double VerticalStart = 40;
        public const double ContentWidthPadding = 80;
        public const double SectionSpacing = 10;

        // font constants
        public const string DefaultFontFamily = "Arial";
        public const double DefaultFontSize = 12;

        // index constants
        public const int FileNameIndexInPath = 1;
        public const int DatePartIndex = 2;

        // file constants
        public const string BaseFolderName = "BookingBoardgames";
        public const string FileDateFormat = "yyMMdd";
        public const string DisplayDateFormat = "dd/MM/yyyy";
        public const string FileNameSeparator = "_";
        public const string DocumentTitle = "Receipt";
    }
}

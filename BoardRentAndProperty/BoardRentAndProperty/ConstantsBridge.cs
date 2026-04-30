// ConstantsBridge — re-exports top-level static-class members from
// `BoardRentAndProperty.Resources.Constants` (the merged-from-PaM root constants class)
// into the `BoardRentAndProperty.Constants` namespace, so PaM's existing call sites of
// the form `Constants.DialogTitles.X` keep working without modification.
//
// Why: the original PaM `Constants.cs` lived in the root namespace `Property_and_Management`.
// The bulk merge would have put it into `BoardRentAndProperty.Constants`, which collides
// with the `BoardRentAndProperty.Constants.DomainConstants` from the PaM `src/Constants/`
// subfolder. Resolution: place the merged-from-root file in `BoardRentAndProperty.Resources`
// and bridge its members back here.
namespace BoardRentAndProperty.Constants
{
    internal static class App
    {
        public const string AppTrayIconUri = global::BoardRentAndProperty.Resources.Constants.AppTrayIconUri;
    }

    internal static class DialogTitles
    {
        public const string ValidationError = global::BoardRentAndProperty.Resources.Constants.DialogTitles.ValidationError;
        public const string RequestFailed = global::BoardRentAndProperty.Resources.Constants.DialogTitles.RequestFailed;
        public const string RentalFailed = global::BoardRentAndProperty.Resources.Constants.DialogTitles.RentalFailed;
        public const string ApproveFailed = global::BoardRentAndProperty.Resources.Constants.DialogTitles.ApproveFailed;
        public const string DeclineFailed = global::BoardRentAndProperty.Resources.Constants.DialogTitles.DeclineFailed;
        public const string OfferFailed = global::BoardRentAndProperty.Resources.Constants.DialogTitles.OfferFailed;
        public const string OfferGameConfirmation = global::BoardRentAndProperty.Resources.Constants.DialogTitles.OfferGameConfirmation;
        public const string ApproveRequestConfirmation = global::BoardRentAndProperty.Resources.Constants.DialogTitles.ApproveRequestConfirmation;
        public const string DeclineRequestConfirmation = global::BoardRentAndProperty.Resources.Constants.DialogTitles.DeclineRequestConfirmation;
        public const string CancelRequestConfirmation = global::BoardRentAndProperty.Resources.Constants.DialogTitles.CancelRequestConfirmation;
        public const string DeleteGameConfirmation = global::BoardRentAndProperty.Resources.Constants.DialogTitles.DeleteGameConfirmation;
        public const string GameRemoved = global::BoardRentAndProperty.Resources.Constants.DialogTitles.GameRemoved;
        public const string CannotDeleteGame = global::BoardRentAndProperty.Resources.Constants.DialogTitles.CannotDeleteGame;
    }

    internal static class DialogButtons
    {
        public const string Ok = global::BoardRentAndProperty.Resources.Constants.DialogButtons.Ok;
        public const string Cancel = global::BoardRentAndProperty.Resources.Constants.DialogButtons.Cancel;
        public const string GoBack = global::BoardRentAndProperty.Resources.Constants.DialogButtons.GoBack;
        public const string Approve = global::BoardRentAndProperty.Resources.Constants.DialogButtons.Approve;
        public const string Decline = global::BoardRentAndProperty.Resources.Constants.DialogButtons.Decline;
        public const string Delete = global::BoardRentAndProperty.Resources.Constants.DialogButtons.Delete;
        public const string CancelRequest = global::BoardRentAndProperty.Resources.Constants.DialogButtons.CancelRequest;
        public const string Offer = global::BoardRentAndProperty.Resources.Constants.DialogButtons.Offer;
    }

    internal static class DialogMessages
    {
        public const string UnexpectedErrorOccurred = global::BoardRentAndProperty.Resources.Constants.DialogMessages.UnexpectedErrorOccurred;
        public const string NoReasonProvided = global::BoardRentAndProperty.Resources.Constants.DialogMessages.NoReasonProvided;
        public const string CreateRequestValidationError = global::BoardRentAndProperty.Resources.Constants.DialogMessages.CreateRequestValidationError;
        public const string CreateRentalValidationError = global::BoardRentAndProperty.Resources.Constants.DialogMessages.CreateRentalValidationError;
    }

    internal static class NotificationTitles
    {
        public const string UpcomingRentalReminder = global::BoardRentAndProperty.Resources.Constants.NotificationTitles.UpcomingRentalReminder;
        public const string BookingUnavailable = global::BoardRentAndProperty.Resources.Constants.NotificationTitles.BookingUnavailable;
        public const string RentalRequestDeclined = global::BoardRentAndProperty.Resources.Constants.NotificationTitles.RentalRequestDeclined;
        public const string RentalRequestCancelled = global::BoardRentAndProperty.Resources.Constants.NotificationTitles.RentalRequestCancelled;
        public const string RentalRequestApproved = global::BoardRentAndProperty.Resources.Constants.NotificationTitles.RentalRequestApproved;
        public const string OfferReceived = global::BoardRentAndProperty.Resources.Constants.NotificationTitles.OfferReceived;
        public const string OfferAccepted = global::BoardRentAndProperty.Resources.Constants.NotificationTitles.OfferAccepted;
        public const string RentalConfirmed = global::BoardRentAndProperty.Resources.Constants.NotificationTitles.RentalConfirmed;
        public const string OfferDenied = global::BoardRentAndProperty.Resources.Constants.NotificationTitles.OfferDenied;
        public const string OfferDeclined = global::BoardRentAndProperty.Resources.Constants.NotificationTitles.OfferDeclined;
    }

    internal static class ValidationMessages
    {
        public const string MaximumPlayerCountComparedToMinimum =
            global::BoardRentAndProperty.Resources.Constants.ValidationMessages.MaximumPlayerCountComparedToMinimum;

        public static string NameLengthRange(int minimumLength, int maximumLength) =>
            global::BoardRentAndProperty.Resources.Constants.ValidationMessages.NameLengthRange(minimumLength, maximumLength);

        public static string PriceMinimum(decimal minimumPrice) =>
            global::BoardRentAndProperty.Resources.Constants.ValidationMessages.PriceMinimum(minimumPrice);

        public static string MinimumPlayerCount(int minimumPlayers) =>
            global::BoardRentAndProperty.Resources.Constants.ValidationMessages.MinimumPlayerCount(minimumPlayers);

        public static string DescriptionLengthRange(int minimumLength, int maximumLength) =>
            global::BoardRentAndProperty.Resources.Constants.ValidationMessages.DescriptionLengthRange(minimumLength, maximumLength);
    }

    internal static class GameValidation
    {
        public const int MinimumNameLength = DomainConstants.GameMinimumNameLength;
        public const int MaximumNameLength = DomainConstants.GameMaximumNameLength;
        public const decimal MinimumAllowedPrice = DomainConstants.GameMinimumAllowedPrice;
        public const int MinimumPlayerCount = DomainConstants.GameMinimumPlayerCount;
        public const int MinimumDescriptionLength = DomainConstants.GameMinimumDescriptionLength;
        public const int MaximumDescriptionLength = DomainConstants.GameMaximumDescriptionLength;
        public const int DefaultMinimumPlayers = DomainConstants.GameDefaultMinimumPlayers;
        public const int DefaultMaximumPlayers = DomainConstants.GameDefaultMaximumPlayers;
    }
}
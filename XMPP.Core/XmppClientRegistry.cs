using System.Reflection;
using System.Xml.Serialization;
using OneOf;
using XMPP.Core.Errors;
using XMPP.Core.Features;

namespace XMPP.Core;

using RegisterFeatureResult = OneOf<
  Unit,
  RegisterFeatureResults.AmbiguousAttributeMatch,
  RegisterFeatureResults.FeatureNamespaceAlreadyRegistered,
  RegisterFeatureResults.FeatureNamespaceMissing
>;

using RegisterClientErrorResult = OneOf<
  Unit,
  RegisterClientErrorResults.AmbiguousAttributeMatch,
  RegisterClientErrorResults.XmlErrorNameMissing,
  RegisterClientErrorResults.XmlErrorNamespaceMissing,
  RegisterClientErrorResults.ErrorAlreadyRegistered
>;

/// <summary>
/// The XmppClientRegistry holds XML serializers that are common across all clients
/// </summary>
public static class XmppClientRegistry
{
  public static Dictionary<string, XmlSerializer> ErrorSerializers { get; } = new();
  
  public static Dictionary<string, XmlSerializer> FeatureSerializers { get; } = new();

  static XmppClientRegistry()
  {
    // Features
    RegisterFeature<StartTlsFeature>();
    RegisterFeature<SaslFeature>();
    RegisterFeature<BindFeature>();

    // Errors - StreamErrors
    RegisterClientError<StreamErrors.BadFormat>();
    RegisterClientError<StreamErrors.BadNamespacePrefix>();
    RegisterClientError<StreamErrors.Conflict>();
    RegisterClientError<StreamErrors.ConnectionTimeout>();
    RegisterClientError<StreamErrors.HostGone>();
    RegisterClientError<StreamErrors.HostUnknown>();
    RegisterClientError<StreamErrors.ImproperAddressing>();
    RegisterClientError<StreamErrors.InternalServerError>();
    RegisterClientError<StreamErrors.InvalidFrom>();
    RegisterClientError<StreamErrors.InvalidNamespace>();
    RegisterClientError<StreamErrors.InvalidXml>();
    RegisterClientError<StreamErrors.NotAuthorized>();
    RegisterClientError<StreamErrors.NotWellFormed>();
    RegisterClientError<StreamErrors.PolicyViolation>();
    RegisterClientError<StreamErrors.RemoteConnectionFailed>();
    RegisterClientError<StreamErrors.Reset>();
    RegisterClientError<StreamErrors.ResourceConstraint>();
    RegisterClientError<StreamErrors.RestrictedXml>();
    RegisterClientError<StreamErrors.SeeOtherHost>();
    RegisterClientError<StreamErrors.SystemShutdown>();
    RegisterClientError<StreamErrors.UndefinedCondition>();
    RegisterClientError<StreamErrors.UnsupportedEncoding>();
    RegisterClientError<StreamErrors.UnsupportedFeature>();
    RegisterClientError<StreamErrors.UnsupportedStanzaType>();
    RegisterClientError<StreamErrors.UnsupportedVersion>();

    // Errors - SaslErrors
    RegisterClientError<SaslErrors.Aborted>();
    RegisterClientError<SaslErrors.AccountDisabled>();
    RegisterClientError<SaslErrors.CredentialsExpired>();
    RegisterClientError<SaslErrors.EncryptionRequired>();
    RegisterClientError<SaslErrors.IncorrectEncoding>();
    RegisterClientError<SaslErrors.InvalidAuthZid>();
    RegisterClientError<SaslErrors.InvalidMechanism>();
    RegisterClientError<SaslErrors.MalformedRequest>();
    RegisterClientError<SaslErrors.MechanismTooWeak>();
    RegisterClientError<SaslErrors.NotAuthorized>();
    RegisterClientError<SaslErrors.TemporaryAuthFailure>();

    // Errors - StanzaErrors
    RegisterClientError<StanzaErrors.BadRequest>();
    RegisterClientError<StanzaErrors.Conflict>();
    RegisterClientError<StanzaErrors.FeatureNotImplemented>();
    RegisterClientError<StanzaErrors.Forbidden>();
    RegisterClientError<StanzaErrors.Gone>();
    RegisterClientError<StanzaErrors.InternalServerError>();
    RegisterClientError<StanzaErrors.ItemNotFound>();
    RegisterClientError<StanzaErrors.JidMalformed>();
    RegisterClientError<StanzaErrors.NotAcceptable>();
    RegisterClientError<StanzaErrors.NotAllowed>();
    RegisterClientError<StanzaErrors.NotAuthorized>();
    RegisterClientError<StanzaErrors.PolicyViolation>();
    RegisterClientError<StanzaErrors.RecipientUnavailable>();
    RegisterClientError<StanzaErrors.Redirect>();
    RegisterClientError<StanzaErrors.RegistrationRequired>();
    RegisterClientError<StanzaErrors.RemoteServerNotFound>();
    RegisterClientError<StanzaErrors.RemoteServerTimeout>();
    RegisterClientError<StanzaErrors.ResourceConstraint>();
    RegisterClientError<StanzaErrors.ServiceUnavailable>();
    RegisterClientError<StanzaErrors.SubscriptionRequired>();
    RegisterClientError<StanzaErrors.UndefinedCondition>();
    RegisterClientError<StanzaErrors.UnexpectedRequest>();
  }
  
  /// <summary>
  /// Registers a stream feature with the registry
  /// </summary>
  /// <typeparam name="T">Feature to register</typeparam>
  public static RegisterFeatureResult RegisterFeature<T>()
  {
    try
    {
      var attr = (XmlRootAttribute?)Attribute.GetCustomAttribute(
        typeof(T), typeof(XmlRootAttribute));

      if (attr?.Namespace == null)
        return new RegisterFeatureResults.FeatureNamespaceMissing();

      if (FeatureSerializers.ContainsKey(attr.Namespace))
        return new RegisterFeatureResults.FeatureNamespaceAlreadyRegistered(attr.Namespace);

      FeatureSerializers.Add(attr.Namespace, new XmlSerializer(typeof(T)));
      return new Unit();
    }
    catch (AmbiguousMatchException)
    {
      return new RegisterFeatureResults.AmbiguousAttributeMatch();
    }
  }

  /// <summary>
  /// Registers a client error with the registry
  /// </summary>
  /// <typeparam name="T">Client error to register</typeparam>
  public static RegisterClientErrorResult RegisterClientError<T>() where T : IClientError
  {
    try
    {
      var attr = (XmlRootAttribute?)Attribute.GetCustomAttribute(
        typeof(T), typeof(XmlRootAttribute));

      if (attr?.ElementName == null)
        return new RegisterClientErrorResults.XmlErrorNameMissing();
      if (attr.Namespace == null)
        return new RegisterClientErrorResults.XmlErrorNamespaceMissing();

      var key = $"{attr.Namespace}/{attr.ElementName}";

      if (ErrorSerializers.ContainsKey(key))
        return new RegisterClientErrorResults.ErrorAlreadyRegistered(key);

      ErrorSerializers.Add(key, new XmlSerializer(typeof(T)));
      return new Unit();
    }
    catch (AmbiguousMatchException)
    {
      return new RegisterClientErrorResults.AmbiguousAttributeMatch();
    }
  }
}
using JasonQuery.Core.Database.Metadata;
using System;

namespace JasonQuery.Core.Database.CreateScript.Oracle
{
    internal sealed class OracleCreateScriptRequest
    {
        public string SchemaType { get; private set; }

        public string SchemaName { get; private set; }

        public string PackageSpecBody { get; private set; }

        public string Owner { get; private set; }

        public string DdlObjectType { get; private set; }

        public string MetadataObjectType { get; private set; }

        public string HeaderObjectText { get; private set; }

        public static OracleCreateScriptRequest Create(string schemaType, string schemaName, string packageSpecBody, string owner, string defaultOwner)
        {
            var request = new OracleCreateScriptRequest
            {
                SchemaType = (schemaType ?? string.Empty).Trim(),
                SchemaName = (schemaName ?? string.Empty).Trim(),
                PackageSpecBody = NormalizePackageSpecBody(packageSpecBody),
                Owner = NormalizeOwner(owner, defaultOwner)
            };

            ResolveObjectTypes(request);

            return request;
        }

        private static string NormalizePackageSpecBody(string packageSpecBody)
        {
            return string.Equals(packageSpecBody, "Body", StringComparison.OrdinalIgnoreCase) ? "Body" : "Spec";
        }

        private static string NormalizeOwner(string owner, string defaultOwner)
        {
            var result = string.IsNullOrWhiteSpace(owner) ? defaultOwner : owner;

            return (result ?? string.Empty).Trim().ToUpperInvariant();
        }

        private static void ResolveObjectTypes(OracleCreateScriptRequest request)
        {
            switch (request.SchemaType)
            {
                case SchemaObjectNames.Tables:
                    {
                        request.DdlObjectType = "TABLE";
                        request.MetadataObjectType = "TABLE";
                        request.HeaderObjectText = "Table";
                        break;
                    }
                case SchemaObjectNames.Views:
                    {
                        request.DdlObjectType = "VIEW";
                        request.MetadataObjectType = "VIEW";
                        request.HeaderObjectText = "View";
                        break;
                    }
                case SchemaObjectNames.Functions:
                    {
                        request.DdlObjectType = "FUNCTION";
                        request.MetadataObjectType = "FUNCTION";
                        request.HeaderObjectText = "Function";
                        break;
                    }
                case SchemaObjectNames.Procedures:
                    {
                        request.DdlObjectType = "PROCEDURE";
                        request.MetadataObjectType = "PROCEDURE";
                        request.HeaderObjectText = "Procedure";
                        break;
                    }
                case SchemaObjectNames.Triggers:
                    {
                        request.DdlObjectType = "TRIGGER";
                        request.MetadataObjectType = "TRIGGER";
                        request.HeaderObjectText = "Trigger";
                        break;
                    }
                case SchemaObjectNames.Indexes:
                    {
                        request.DdlObjectType = "INDEX";
                        request.MetadataObjectType = "INDEX";
                        request.HeaderObjectText = "Index";
                        break;
                    }
                case SchemaObjectNames.Packages:
                    {
                        if (string.Equals(request.PackageSpecBody, "Body", StringComparison.OrdinalIgnoreCase))
                        {
                            request.DdlObjectType = "PACKAGE_BODY";
                            request.MetadataObjectType = "PACKAGE BODY";
                            request.HeaderObjectText = "Package Body";
                        }
                        else
                        {
                            request.DdlObjectType = "PACKAGE";
                            request.MetadataObjectType = "PACKAGE";
                            request.HeaderObjectText = "Package";
                        }

                        break;
                    }
                default:
                    {
                        throw new NotSupportedException($"Oracle GetCreateScript 尚未支援 SchemaType: {request.SchemaType}");
                    }
            }
        }
    }
}
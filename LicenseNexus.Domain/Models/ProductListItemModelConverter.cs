using System.Text.Json;
using System.Text.Json.Serialization;

namespace LicenseNexus.Domain.Models;

public class ProductListItemModelConverter : JsonConverter<ProductListItemModel>
{
    public override ProductListItemModel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Очікувався початок об'єкта.");
        }

        var model = new ProductListItemModel();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return model;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                string? propertyName = reader.GetString();
                reader.Read();

                if (propertyName == "Id" || propertyName == "id")
                {
                    model.Id = reader.GetInt32();
                }
                else if (propertyName == "Sku" || propertyName == "sku")
                {
                    model.Sku = reader.GetString() ?? string.Empty;
                }
                else if (propertyName == "Title" || propertyName == "title")
                {
                    model.Title = reader.GetString() ?? string.Empty;
                }
                else if (propertyName == "Classification" || propertyName == "classification")
                {
                    ReadClassification(ref reader, model);
                }
                else if (propertyName == "Prices" || propertyName == "prices")
                {
                    ReadPrices(ref reader, model);
                }
                else if (propertyName == "Attributes" || propertyName == "attributes")
                {
                    if (reader.TokenType == JsonTokenType.StartObject)
                    {
                        int attrDepth = reader.CurrentDepth;
                        while (reader.Read() && reader.CurrentDepth > attrDepth)
                        {
                            if (reader.TokenType == JsonTokenType.PropertyName)
                            {
                                string? attrProp = reader.GetString();
                                reader.Read();
                                if (attrProp == "IsPromo" || attrProp == "isPromo") model.IsPromo = reader.GetBoolean();
                                else if (attrProp == "IsTop" || attrProp == "isTop") model.IsTop = reader.GetBoolean();
                                else if (attrProp == "IsNew" || attrProp == "isNew") model.IsNew = reader.GetBoolean();
                                else if (attrProp == "Logo" || attrProp == "logo") model.Logo = reader.GetString();
                                else reader.Skip();
                            }
                        }
                    }
                    else reader.Skip();
                }
                else if (propertyName == "Currency" || propertyName == "currency")
                {
                    if (reader.TokenType == JsonTokenType.StartObject)
                    {
                        int currDepth = reader.CurrentDepth;
                        while (reader.Read() && reader.CurrentDepth > currDepth)
                        {
                            if (reader.TokenType == JsonTokenType.PropertyName)
                            {
                                string? currProp = reader.GetString();
                                reader.Read();
                                if (currProp == "LiteralCode" || currProp == "literalCode") model.CurrencyLiteralCode = reader.GetString() ?? string.Empty;
                                else reader.Skip();
                            }
                        }
                    }
                    else reader.Skip();
                }
                else
                {
                    reader.Skip(); 
                }
            }
        }

        throw new JsonException("Несподіваний кінець JSON.");
    }

    private void ReadClassification(ref Utf8JsonReader reader, ProductListItemModel model)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            reader.Skip();
            return;
        }

        int initialDepth = reader.CurrentDepth;

        while (reader.Read() && reader.CurrentDepth > initialDepth)
        {
            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                string? prop = reader.GetString();
                reader.Read();

                if (prop == "Vendor" || prop == "vendor")
                {
                    int vendorDepth = reader.CurrentDepth;
                    while (reader.Read() && reader.CurrentDepth > vendorDepth)
                    {
                        if (reader.TokenType == JsonTokenType.PropertyName && (reader.GetString() == "Name" || reader.GetString() == "name"))
                        {
                            reader.Read();
                            model.VendorName = reader.GetString() ?? string.Empty;
                        }
                        else if (reader.TokenType == JsonTokenType.PropertyName)
                        {
                            reader.Skip();
                        }
                    }
                }
                else if (prop == "Group" || prop == "group")
                {
                    int groupDepth = reader.CurrentDepth;
                    while (reader.Read() && reader.CurrentDepth > groupDepth)
                    {
                        if (reader.TokenType == JsonTokenType.PropertyName && (reader.GetString() == "CategoryName" || reader.GetString() == "categoryName"))
                        {
                            reader.Read();
                            model.CategoryName = reader.GetString() ?? string.Empty;
                        }
                        else if (reader.TokenType == JsonTokenType.PropertyName && (reader.GetString() == "Name" || reader.GetString() == "name"))
                        {
                            reader.Read();
                            model.GroupName = reader.GetString() ?? string.Empty;
                        }
                        else if (reader.TokenType == JsonTokenType.PropertyName)
                        {
                            reader.Skip();
                        }
                    }
                }
                else
                {
                    reader.Skip();
                }
            }
        }
    }

    private void ReadPrices(ref Utf8JsonReader reader, ProductListItemModel model)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            reader.Skip();
            return;
        }

        bool isFirstPriceFound = false;
        int initialDepth = reader.CurrentDepth;

        while (reader.Read() && reader.CurrentDepth > initialDepth)
        {
            if (reader.TokenType == JsonTokenType.StartObject && !isFirstPriceFound)
            {
                int objDepth = reader.CurrentDepth;
                while (reader.Read() && reader.CurrentDepth > objDepth)
                {
                    if (reader.TokenType == JsonTokenType.PropertyName && (reader.GetString() == "Price" || reader.GetString() == "price"))
                    {
                        reader.Read();
                        model.BasePrice = reader.GetDecimal();
                        isFirstPriceFound = true;
                    }
                    else if (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        reader.Skip();
                    }
                }
            }
        }
    }

    public override void Write(Utf8JsonWriter writer, ProductListItemModel value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("id", value.Id);
        writer.WriteString("sku", value.Sku);
        writer.WriteString("title", value.Title);
        writer.WriteBoolean("isPromo", value.IsPromo);
        writer.WriteBoolean("isTop", value.IsTop);
        writer.WriteBoolean("isNew", value.IsNew);
        writer.WriteString("logo", value.Logo);
        writer.WriteString("vendorName", value.VendorName);
        writer.WriteString("categoryName", value.CategoryName);
        writer.WriteString("groupName", value.GroupName);
        writer.WriteNumber("basePrice", value.BasePrice);
        writer.WriteString("currencyLiteralCode", value.CurrencyLiteralCode);
        writer.WriteEndObject();
    }
}
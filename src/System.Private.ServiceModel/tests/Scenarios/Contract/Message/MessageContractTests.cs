// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MessageContractCommon;
using System;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;
using Xunit;

public static class MessageContractTests
{
    [Fact]
    [OuterLoop]
    public static void MessageContract_IsWrapped_True()
    {
        MessageContractHelpers.IMessageContract clientProxy;
        MessageContractTypes.RequestBankingData requestData;
        MyInspector inspector = MessageContractHelpers.SetupMessageContractTests(out clientProxy, out requestData);
        clientProxy.MessageContractRequestReply(requestData);
        XmlDictionaryReader reader = MessageContractHelpers.GetResponseBodyReader(inspector);

        Assert.True(reader.LocalName.Equals(MessageContractConstants.wrapperName),
            string.Format("reader.LocalName - Expected: {0}, Actual: {1}", MessageContractConstants.wrapperName, reader.LocalName));

        Assert.True(reader.NamespaceURI.Equals(MessageContractConstants.wrapperNamespace),
            string.Format("reader.NamespaceURI - Expected: {0}, Actual: {1}", MessageContractConstants.wrapperNamespace, reader.NamespaceURI));
    }

    [Fact]
    [OuterLoop]
    public static void MessageContract_IsWrapped_False()
    {
        MessageContractHelpers.IMessageContract clientProxy;
        MessageContractTypes.RequestBankingData requestData;
        MyInspector inspector = MessageContractHelpers.SetupMessageContractTests(out clientProxy, out requestData);
        clientProxy.MessageContractRequestReplyNotWrapped(requestData);
        XmlDictionaryReader reader = MessageContractHelpers.GetResponseBodyReader(inspector);

        Assert.False(reader.LocalName.Equals(MessageContractConstants.wrapperName),
            "When IsWrapped set to false, the message body should not be wrapped with an extra element.");
    }

    [Fact]
    [OuterLoop]
    public static void MessageBody_Elements_Ordered()
    {
        MessageContractHelpers.IMessageContract clientProxy;
        MessageContractTypes.RequestBankingData requestData;
        MyInspector inspector = MessageContractHelpers.SetupMessageContractTests(out clientProxy, out requestData);
        clientProxy.MessageContractRequestReply(requestData);
        XmlDictionaryReader reader = MessageContractHelpers.GetResponseBodyReader(inspector);

        Assert.True(reader.LocalName.Equals(MessageContractConstants.wrapperName),
            string.Format("Unexpected element order (1/5). Expected {0}, Actual: {1}", MessageContractConstants.wrapperName, reader.LocalName));

        reader.Read();

        Assert.True(reader.LocalName.Equals(MessageContractConstants.dateElementName),
            string.Format("Unexpected element order (2/5). Expected {0}, Actual: {1}", MessageContractConstants.dateElementName, reader.LocalName));

        reader.Read(); // Move to Value node
        reader.Read(); // Move to the end tag
        reader.ReadEndElement(); // Checks that the current content node is an end tag and advances the reader to the next node.

        Assert.True(reader.LocalName.Equals(MessageContractConstants.transactionElementName),
            string.Format("Unexpected element order (3/5). Expected: {0}, Actual: {1}", MessageContractConstants.transactionElementName, reader.LocalName));

        reader.Read(); // Move to Value node
        reader.Read(); // Move to the end tag
        reader.ReadEndElement(); // Checks that the current content node is an end tag and advances the reader to the next node.

        Assert.True(reader.LocalName.Equals(MessageContractConstants.customerElementName),
            string.Format("Unexpected element order (4/5). Expected: {0}, Actual: {1}", MessageContractConstants.customerElementName, reader.LocalName));


        reader.Read(); // Move to Value node
        reader.Read(); // Move to the end tag
        reader.ReadEndElement(); // Checks that the current content node is an end tag and advances the reader to the next node.

        Assert.True(reader.IsStartElement() == false && reader.LocalName.Equals(MessageContractConstants.wrapperName),
            string.Format("Unexpected element order (5/5). Expected: {0}, Actual: {1}", MessageContractConstants.wrapperName, reader.LocalName));
    }

    [Fact]
    [OuterLoop]
    public static void MessageBody_Elements_CustomerElement_Value_Matches()
    {
        MessageContractHelpers.IMessageContract clientProxy;
        MessageContractTypes.RequestBankingData requestData;
        MyInspector inspector = MessageContractHelpers.SetupMessageContractTests(out clientProxy, out requestData);
        clientProxy.MessageContractRequestReply(requestData);
        XmlDictionaryReader reader = MessageContractHelpers.GetResponseBodyReader(inspector);

        bool elementFound = false;
        while (reader.Read())
        {
            if (reader.LocalName.Equals(MessageContractConstants.customerElementName) && reader.NamespaceURI.Equals(MessageContractConstants.customerElementNamespace))
            {
                elementFound = true;
                reader.ReadStartElement();
                Assert.Equal(reader.Value, MessageContractConstants.customerElementValue);
                break;
            }
            else
            {
                // Continue checking remaining nodes.
            }
        }

        Assert.True(elementFound,
            string.Format("Expected element not found. Looking For: {0} && {1}", MessageContractConstants.customerElementName, MessageContractConstants.customerElementNamespace));

    }

    [Fact]
    [OuterLoop]
    public static void MessageHeader_MustUnderstand_True()
    {
        MessageContractHelpers.IMessageContract clientProxy;
        MessageContractTypes.RequestBankingData requestData;
        MyInspector inspector = MessageContractHelpers.SetupMessageContractTests(out clientProxy, out requestData);
        clientProxy.MessageContractRequestReplyWithMessageHeader(requestData);
        MessageHeaders headers = MessageContractHelpers.GetHeaders(inspector);

        int index = headers.FindHeader(MessageContractConstants.extraValuesName, MessageContractConstants.extraValuesNamespace);
        var header = headers[index];

        Assert.True(header != null, "There's no header in the message.");
        Assert.True(header.MustUnderstand, "Expected MustUnderstand to be true, but it was false.");
    }

    [Fact]
    [OuterLoop]
    public static void MessageHeader_MustUnderstand_False()
    {
        MessageContractHelpers.IMessageContract clientProxy;
        MessageContractTypes.RequestBankingData requestData;
        MyInspector inspector = MessageContractHelpers.SetupMessageContractTests(out clientProxy, out requestData);
        clientProxy.MessageContractRequestReplyWithMessageHeaderNotNecessaryUnderstood(requestData);
        MessageHeaders headers = MessageContractHelpers.GetHeaders(inspector);

        int index = headers.FindHeader(MessageContractConstants.extraValuesName, MessageContractConstants.extraValuesNamespace);
        var header = headers[index];

        Assert.True(header != null, "There's no header in the message.");
        Assert.False(header.MustUnderstand, "Expected MustUnderstand to be false, but it was true.");
    }
}

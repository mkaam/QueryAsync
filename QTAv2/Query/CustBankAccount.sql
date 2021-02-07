SELECT       SUBSTRING(DB_NAME(),14,3) iSMSAreaCode, Customer.CustomerCode, Customer.CustomerName, Customer.CustomerStatus, Customer.CustomerRegisteredName, CustomerType.CustomerTypeCode, InternalClassificationLevel.InternalClassificationLevelCode, Contact.ContactFirstName, 
                         Contact.ContactLastName, ContactType.ContactTypeCode, Customer.CustomerBankAccountName1, Customer.CustomerBankAccountNumber1
FROM            Customer INNER JOIN
                         CustomerType ON Customer.Customer_CustomerTypeId = CustomerType.CustomerTypeId LEFT OUTER JOIN
                         InternalClassificationLevel ON Customer.Customer_InternalClassificationLevelId = InternalClassificationLevel.InternalClassificationLevelId INNER JOIN
                         Contact ON Customer.CustomerId = Contact.Contact_CustomerId INNER JOIN
                         ContactType ON Contact.Contact_ContactTypeId = ContactType.ContactTypeId
						 inner join Users.dbo.CustBankAccount a on (Customer.CustomerCode = a.CustomerCode)
where CustomerStatus = 'A'
ORDER BY CustomerCode ASC
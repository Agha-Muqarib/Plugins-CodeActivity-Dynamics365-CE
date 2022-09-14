using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PluginAssignment
{

    public class Contact : IPlugin
    {
        ITracingService tracingService;
        IOrganizationService service;
        IPluginExecutionContext context;
        Entity entity;

        public void Execute(IServiceProvider serviceProvider)
        {
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = serviceFactory.CreateOrganizationService(context.UserId);

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                entity = (Entity)context.InputParameters["Target"];

                try
                {
                    if (entity.LogicalName == "contact")
                    {

                        tracingService.Trace("Reached if after contact");
                        switch (context.Stage)
                        {
                            case 10://Pre validation
                                tracingService.Trace("Reached case 10");
                                switch (context.MessageName)
                                {

                                    case "Create":

                                        tracingService.Trace("Reached create of switch 10");

                                        // Call Task 01
                                        CompareIntOnCreate();


                                        tracingService.Trace("Task01 called");

                                        break;

                                    case "Update":

                                       tracingService.Trace("Reached update of switch 10");
                                        CompareIntOnUpdate();
                                      tracingService.Trace(" update of switch 10 executed successfully");
//

                                        break;

                                    default:
                                        break;
                                }
                                break;

                            case 20://Pre-operation

                                switch (context.MessageName)
                                {
                                    case "Create":
                                        break;

                                    case "Update":
                                        break;

                                    default:
                                        break;
                                }
                                break;

                            case 40://post-operation

                                switch (context.MessageName)
                                {
                                    case "Create":   
                                        tracingService.Trace("Reached create of 40");

                                        SetStringField();
                                        tracingService.Trace("task 02 executed sussessfully");

                                        tracingService.Trace("starting task 04");


                                        SetIdOnLookup();
                                        tracingService.Trace("task 04 executed sussessfully");

                                        break;

                                    case "Update":
                                        
                                        tracingService.Trace("Reached update of 40");
                                        // call task 03 on update of post operation
                                        FetchFieldStuff();
                                        tracingService.Trace("task 03 executed sussessfully");
                                        break;

                                    default:
                                        break;
                                }
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidPluginExecutionException(ex.Message);
                }
            }
        }
        private void CompareIntOnCreate()
        {
            tracingService.Trace("entered task 01 ");

            int allowedAge = entity.GetAttributeValue<int>("plas_allowedage");
            tracingService.Trace("allowed age: ", allowedAge );

            int age = entity.GetAttributeValue<int>("plas_age");
            tracingService.Trace("age: ", age );


            if (allowedAge > age)
            {
                tracingService.Trace("entered if of task 01 ");

                throw new InvalidPluginExecutionException("Age is less than allowed age.");
                        
                tracingService.Trace("TASK 01 COMPLETED");

            }
           
        }

        private void CompareIntOnUpdate()
        {
             try
             {
                 tracingService.Trace("entered try");

                 int IV_age;
                 int IV_allowedAge;

                 int age = entity.GetAttributeValue<Int32>("plas_age");
                 int allowedAge = entity.GetAttributeValue<Int32>("plas_allowedage");

                 if (context.PreEntityImages.Contains("PreImage") && context.PreEntityImages["PreImage"] is Entity)
                 {
                     tracingService.Trace("entered first if");
                     Entity PreImage = context.PreEntityImages["PreImage"];

                     if (PreImage.Attributes.Contains("plas_age") && PreImage.Attributes.Contains("plas_allowedage"))
                     {
                         tracingService.Trace("entered second if");
                        
                         IV_allowedAge = PreImage.GetAttributeValue<Int32>("plas_allowedage");      // int1
                         tracingService.Trace("IV_allowedAge = ", IV_allowedAge);
                         IV_age = PreImage.GetAttributeValue<Int32>("plas_age");                   // int2
                         tracingService.Trace("IV_Age = ", IV_age);

                        if (age != 0 && allowedAge == 0)
                         {

                            // When only age is updated
                             tracingService.Trace("first case");
                            tracingService.Trace("Age = ", age);
                            tracingService.Trace("Allowed Age = ", allowedAge);


                            if (IV_allowedAge > age)
                             {
                                 tracingService.Trace("first case if");
                            tracingService.Trace("Age = ", age);
                                tracingService.Trace("IV_allowedAge = ", IV_allowedAge);

                                throw new InvalidPluginExecutionException("Age must be greater than allowed age");
                             }
                         }
                         
                         else if (age == 0 && allowedAge != 0)
                         {
                            // when only allowed age is updated
                             tracingService.Trace("second case");
                             if (allowedAge > IV_age)
                             {
                                 tracingService.Trace("second case if ");
                                 throw new InvalidPluginExecutionException("Age must be greater than allowed age");
                             }
                         }
                         
                         else if (age != 0 && allowedAge != 0)
                         {
                            // when both are updated
                             tracingService.Trace("third case");
                             if (allowedAge > age)
                             {
                                 tracingService.Trace("third case if");
                                 throw new InvalidPluginExecutionException("Age must be greater than allowed age");
                             }
                         }
                     }
                 }
             }

             catch (InvalidPluginExecutionException ex)
             {
                 throw new InvalidPluginExecutionException(ex.Message);
             }
         }


        private void SetStringField()
        {
            tracingService.Trace("entered task 02 ");

            //    tracingService.Trace("entered if of task 02 ", Convert.ToString(entity.GetAttributeValue<string>("parentcustomerid")));

            try
            {
                tracingService.Trace("try entered");

                Guid id = entity.GetAttributeValue<EntityReference>("parentcustomerid").Id;
                tracingService.Trace("id =  ", id.ToString());

                var cols = new ColumnSet("parentcustomerid");
                tracingService.Trace("reached cols");
                
                Entity parententity = service.Retrieve("contact", id, cols);

                tracingService.Trace("reached retrieve");

                string name = ((EntityReference)parententity.Attributes["parentcustomerid"]).Name;
                tracingService.Trace("name = ", name);

                entity.Attributes["plas_stringfield"] = name;
                service.Update(entity);

            }

            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }
        } 

        private void FetchFieldStuff()
        {
            tracingService.Trace("Entered task 03");
            QueryExpression queryExpression = new QueryExpression("contact"); 
            queryExpression.ColumnSet = new ColumnSet(true); 

            EntityCollection entityCollection = service.RetrieveMultiple(queryExpression);
            
            tracingService.Trace("Reached before loop");

            // get attribute name, attribute type, attribute value of a single entity

            foreach (Entity entity in entityCollection.Entities)
            {
                tracingService.Trace("Reached inside loop");

                foreach (KeyValuePair<string, object> attribute in entity.Attributes)
                {

                    tracingService.Trace("XXXXXXXXXXXXXXXXX");

                    string attributeName = attribute.Key;
                    //tracingService.Trace("attribute name = ", attributeName);

                    string attributeType = attribute.Value.GetType().ToString();
                   // tracingService.Trace("attribute type = ", attributeType);

                    string attributeValue = attribute.Value.ToString();
                   // tracingService.Trace("attribute value = ", attributeValue);
                    
                   tracingService.Trace("Field name is {0} with type {1} and its value is {2}.", attributeName, attributeType, attributeValue);

                }

               
            }

        }
        private void SetIdOnLookup()
        {
            tracingService.Trace("entered task 04 ");

            try
            {
                tracingService.Trace("try entered");

                // get guid of the newly created contact record

                Guid id = entity.Id;
                 string id1 = id.ToString();
                tracingService.Trace("id = {0} ", id1);

                
                Entity accountEntity = new Entity("account");
                tracingService.Trace("account created");
                accountEntity.Attributes["primarycontactid"] = new EntityReference(entity.LogicalName, id);
                accountEntity.Attributes["name"] = entity.GetAttributeValue<String>("name");
                accountEntity.Attributes["emailaddress1"] = entity.GetAttributeValue<String>("address1_addressid");
                accountEntity.Attributes["address1_line1"] = entity.GetAttributeValue<String>("address1_line1");
                accountEntity.Attributes["address1_city"] = entity.GetAttributeValue<String>("address1_city");
                accountEntity.Attributes["address1_stateorprovince"] = entity.GetAttributeValue<String>("address1_stateorprovince");
                accountEntity.Attributes["address1_postalcode"] = entity.GetAttributeValue<String>("address1_postalcode");

                service.Create(accountEntity);
            }


            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }
                    
        }    
    
    }
}



    
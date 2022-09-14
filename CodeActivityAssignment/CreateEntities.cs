using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Xrm.Sdk;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IdentityModel.Metadata;
using System.Runtime.Remoting.Contexts;
using System.Runtime.ConstrainedExecution;
using System.Threading;
using Microsoft.Xrm.Sdk.Query;

namespace CodeActivityAssignment
{
    public class CreateEntities : CodeActivity
    {

        [Output("SetParentOrder")]
        [ReferenceTarget("plas_order")]
        public OutArgument<EntityReference> SetParentOrder { get; set; }

        // Teacing Service

        ITracingService tracingService;

        protected override void Execute(CodeActivityContext executionContext)
        {
            //Create the tracing service
            ITracingService tracingService = executionContext.GetExtension<ITracingService>();

            //Create the context
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            try
            {

                tracingService.Trace("Entered Try");

                tracingService.Trace("primaryEntityName: " + context.PrimaryEntityName);

                // get data of current entity record

                Entity currentEntityRecord = service.Retrieve(context.PrimaryEntityName, context.PrimaryEntityId, new ColumnSet(true));
                tracingService.Trace("current entity retrieved ");
                

                //  XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX    Order    XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

                if (context.PrimaryEntityName == "plas_order")
                {

                    tracingService.Trace("Entered Order");
                        
                    Entity newOrderEntity = new Entity("plas_order");

                    tracingService.Trace("newOrderEntity created");
                    
                    // Setting fields of new order entity

                    newOrderEntity.Attributes["plas_name"] = currentEntityRecord["plas_name"];
                    newOrderEntity.Attributes["plas_contact"] = currentEntityRecord["plas_contact"];
                    newOrderEntity.Attributes["plas_deliverytime"] = currentEntityRecord["plas_deliverytime"];
                    newOrderEntity.Attributes["plas_orderdate"] = currentEntityRecord["plas_orderdate"];
                    newOrderEntity.Attributes["plas_status"] = currentEntityRecord["plas_status"];

                    // set parent order

                    SetParentOrder.Set(executionContext, newOrderEntity.Attributes["plas_parentorder"] = new EntityReference(context.PrimaryEntityName, context.PrimaryEntityId));

                    tracingService.Trace("current order entity data set to new order entity ");

                    // create new order entity

                    service.Create(newOrderEntity);
                    tracingService.Trace("new order entity created ");
                }

                
                //  XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX    Order Product   XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX


                if (context.PrimaryEntityName == "plas_order")
                {

                    tracingService.Trace("Entered Order");

                    Entity newOpEntity = new Entity("plas_orderproduct");

                    tracingService.Trace("newOrderEntity created");

                    // Setting fields of new order entity

                    newOpEntity.Attributes["plas_name"] = currentEntityRecord["plas_name"];
                    newOpEntity.Attributes["plas_order"] = currentEntityRecord["plas_order"];
                    newOpEntity.Attributes["plas_product"] = currentEntityRecord["plas_product"];
                    newOpEntity.Attributes["plas_orderedquantity"] = currentEntityRecord["plas_orderedquantity"];
                    newOpEntity.Attributes["plas_deliveredquantity"] = currentEntityRecord["plas_deliveredquantity"];
                    newOpEntity.Attributes["plas_remainingquantity"] = currentEntityRecord["plas_remainingquantity"];
                    newOpEntity.Attributes["plas_status"] = currentEntityRecord["plas_status"];


                    tracingService.Trace("current order product entity data set to new order product entity ");

                    // create new order entity

                    service.Create(newOpEntity);
                    tracingService.Trace("new order entity created ");
                }
            }
            
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}







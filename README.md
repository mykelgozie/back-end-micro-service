Electricity Billing API
This API manages electricity bills, payments, and wallet transactions, with event-driven notifications using mock publisher and consumer.

Features
Create Bill: Generates a new electricity bill.
Pay Bill: Processes payments against the wallet if funds are sufficient.
Add Funds: Adds funds to the user wallet.

Event-Driven Notifications: Publishes and subscribes to events when bills are created, payments are processed, and funds are added to wallets.
The event-driven pattern is used for its real-time responsiveness and scalability, allowing services to respond independently and immediately to events. 
It promotes loose coupling, so services are easier to maintain and evolve without interdependency issues. Additionally, this pattern supports fault isolation and resilience, as one service’s failure doesn’t cascade to others. Overall, it enhances flexibility, responsiveness, and cost-efficiency in distributed systems.

Domain Driven pattern 
The domain-driven pattern (DDD) is used to create software closely aligned with complex business needs. By fostering a shared language between developers and domain experts, DDD improves communication and clarity. It structures systems into bounded contexts for modularity, focuses on core domain logic to maximize business value, and provides flexibility to adapt to evolving requirements, making it ideal for complex and evolving domains.

Run the project Databased is embedded in the project using SQL Lite 




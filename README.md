# Project.Dashboard

Sample application to test out the `AspnetCore.HealthChecks` nuget packages.

This will deploy the `Website`, `API` and `Healthchecks` C# projects to local kubernetes.
As part of this, `RabbitMQ` and `Redis` are also installed.

> Note: If running this locally, make sure that you've first installed the `RabbitMQ Operator` to your cluster first:
> kubectl apply -f https://github.com/rabbitmq/cluster-operator/releases/latest/download/cluster-operator.yml

Then, you should be able to run `garden deploy --env local --forward`


> Note: If your healthchecks application throws an error about not having permissions to do service discovery, then you might need to run this:
> kubectl create clusterrolebinding --user system:serviceaccount:project-dashboard-default:default kube-system-cluster-admin --clusterrole cluster-admin

This will amend the role binding for the service account that is used when deploying access to `list services`.
Please note, that this command grants admin permissions - use with caution.



# URLS

http://localhost:9020/healthchecks-ui - this is the healthcheck dashboard

http://localhost:9010 - this is the sample website

http://localhist:9000 - this is the sample REST api


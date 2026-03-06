> test info



test suite: `nbomber_default_test_suite_name`

test name: `nbomber_default_test_name`

session id: `2026-03-06_16-18-50_3f8a74d9`

> scenario stats



scenario: `checkout_scenario`

  - ok count: `38271`

  - fail count: `0`

  - all data: `0` MB

  - duration: `00:02:30`

load simulations:

  - `ramping_constant`, copies: `50`, during: `00:00:30`

  - `keep_constant`, copies: `50`, during: `00:02:00`

|step|ok stats|
|---|---|
|name|`global information`|
|request count|all = `38271`, ok = `38271`, RPS = `255.14`|
|latency (ms)|min = `28.23`, mean = `175.62`, max = `485.14`, StdDev = `75.72`|
|latency percentile (ms)|p50 = `172.8`, p75 = `236.03`, p95 = `296.19`, p99 = `342.02`|


> status codes for scenario: `checkout_scenario`



|status code|count|message|
|---|---|---|
|Order_Placed|38271||



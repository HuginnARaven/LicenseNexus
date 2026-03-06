> test info



test suite: `nbomber_default_test_suite_name`

test name: `nbomber_default_test_name`

session id: `2026-03-06_15-28-39_f8ee7ce9`

> scenario stats



scenario: `checkout_scenario`

  - ok count: `1819`

  - fail count: `5090`

  - all data: `0` MB

  - duration: `00:00:23`

load simulations:

  - `ramping_constant`, copies: `50`, during: `00:00:30`

  - `keep_constant`, copies: `50`, during: `00:02:00`

|step|ok stats|
|---|---|
|name|`global information`|
|request count|all = `6909`, ok = `1819`, RPS = `79.09`|
|latency (ms)|min = `22.85`, mean = `94.18`, max = `319.62`, StdDev = `49.51`|
|latency percentile (ms)|p50 = `80`, p75 = `120.45`, p95 = `195.33`, p99 = `241.66`|


|step|failures stats|
|---|---|
|name|`global information`|
|request count|all = `6909`, fail = `5090`, RPS = `221.3`|
|latency (ms)|min = `8.28`, mean = `48.15`, max = `270.19`, StdDev = `37.19`|
|latency percentile (ms)|p50 = `30.18`, p75 = `65.66`, p95 = `125.12`, p99 = `172.29`|


> status codes for scenario: `checkout_scenario`



|status code|count|message|
|---|---|---|
|Order_Placed|1819||
|InternalServerError|5090|Failed to add order item|



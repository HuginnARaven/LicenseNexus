> test info



test suite: `nbomber_default_test_suite_name`

test name: `nbomber_default_test_name`

session id: `2026-03-05_16-11-32_48421469`

> scenario stats



scenario: `checkout_scenario`

  - ok count: `0`

  - fail count: `5462`

  - all data: `0` MB

  - duration: `00:00:14`

load simulations:

  - `ramping_constant`, copies: `50`, during: `00:00:30`

  - `keep_constant`, copies: `50`, during: `00:02:00`

|step|ok stats|
|---|---|
|name|`global information`|
|request count|all = `5462`, ok = `0`, RPS = `0`|
|latency (ms)|min = `0`, mean = `0`, max = `0`, StdDev = `0`|
|latency percentile (ms)|p50 = `0`, p75 = `0`, p95 = `0`, p99 = `0`|


|step|failures stats|
|---|---|
|name|`global information`|
|request count|all = `5462`, fail = `5462`, RPS = `390.14`|
|latency (ms)|min = `9.26`, mean = `27.09`, max = `416.25`, StdDev = `12.39`|
|latency percentile (ms)|p50 = `25.1`, p75 = `31.57`, p95 = `44.86`, p99 = `64.26`|


> status codes for scenario: `checkout_scenario`



|status code|count|message|
|---|---|---|
|InternalServerError|5462|Failed to add order item|



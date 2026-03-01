> test info



test suite: `nbomber_default_test_suite_name`

test name: `nbomber_default_test_name`

session id: `2026-03-01_23-03-59_34119ef6`

> scenario stats



scenario: `vendor_mutator`

  - ok count: `600`

  - fail count: `0`

  - all data: `0` MB

  - duration: `00:02:00`

load simulations:

  - `inject`, rate: `5`, interval: `00:00:01`, during: `00:02:00`

|step|ok stats|
|---|---|
|name|`global information`|
|request count|all = `600`, ok = `600`, RPS = `5`|
|latency (ms)|min = `2.21`, mean = `4.24`, max = `167.11`, StdDev = `8.71`|
|latency percentile (ms)|p50 = `3.34`, p75 = `4.14`, p95 = `6.16`, p99 = `10.26`|




> scenario stats



scenario: `consistency_watcher`

  - ok count: `493539`

  - fail count: `700`

  - all data: `0` MB

  - duration: `00:02:00`

load simulations:

  - `keep_constant`, copies: `10`, during: `00:02:00`

|step|ok stats|
|---|---|
|name|`global information`|
|request count|all = `494239`, ok = `493539`, RPS = `4112.82`|
|latency (ms)|min = `1.15`, mean = `2.42`, max = `46.52`, StdDev = `0.82`|
|latency percentile (ms)|p50 = `2.19`, p75 = `2.52`, p95 = `3.91`, p99 = `5.71`|


|step|failures stats|
|---|---|
|name|`global information`|
|request count|all = `494239`, fail = `700`, RPS = `5.83`|
|latency (ms)|min = `1.34`, mean = `2.43`, max = `13.83`, StdDev = `0.99`|
|latency percentile (ms)|p50 = `2.17`, p75 = `2.49`, p95 = `3.9`, p99 = `5.4`|


> status codes for scenario: `consistency_watcher`



|status code|count|message|
|---|---|---|
|In_Sync|491818||
|Not_Mutated_Yet|1721||
|Out_Of_Sync|700|Stale Data|



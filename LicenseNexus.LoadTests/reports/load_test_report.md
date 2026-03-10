> test info



test suite: `nbomber_default_test_suite_name`

test name: `nbomber_default_test_name`

session id: `2026-03-10_23-25-57_32f95af0`

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
|latency (ms)|min = `2.22`, mean = `5.05`, max = `50.07`, StdDev = `5.54`|
|latency percentile (ms)|p50 = `3.54`, p75 = `4.41`, p95 = `12.49`, p99 = `34.59`|




> scenario stats



scenario: `consistency_watcher`

  - ok count: `430526`

  - fail count: `2824`

  - all data: `0` MB

  - duration: `00:02:00`

load simulations:

  - `keep_constant`, copies: `10`, during: `00:02:00`

|step|ok stats|
|---|---|
|name|`global information`|
|request count|all = `433350`, ok = `430526`, RPS = `3587.72`|
|latency (ms)|min = `1.15`, mean = `2.76`, max = `73.82`, StdDev = `2.99`|
|latency percentile (ms)|p50 = `2.31`, p75 = `2.64`, p95 = `4.31`, p99 = `8.03`|


|step|failures stats|
|---|---|
|name|`global information`|
|request count|all = `433350`, fail = `2824`, RPS = `23.53`|
|latency (ms)|min = `1.25`, mean = `2.9`, max = `54.31`, StdDev = `3.88`|
|latency percentile (ms)|p50 = `2.31`, p75 = `2.67`, p95 = `4.27`, p99 = `9.57`|


> status codes for scenario: `consistency_watcher`



|status code|count|message|
|---|---|---|
|In_Sync|424223||
|Not_Mutated_Yet|6303||
|Out_Of_Sync|2824|Stale Data|



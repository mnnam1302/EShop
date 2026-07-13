-- KEYS[1]: sliding window counter hash key
-- ARGV[1]: limit (max requests admitted per window)
-- ARGV[2]: window size in seconds
-- Reply: (allowed, remaining, retry_after_ms)
-- Weighted sliding window: estimated_count = curr_count + prev_count * overlap_ratio

local key = KEYS[1]
local limit = tonumber(ARGV[1])
local window = tonumber(ARGV[2])

local time_result = redis.call('TIME')
local now_seconds = tonumber(time_result[1]) + (tonumber(time_result[2]) / 1000000)

local current_window_id = math.floor(now_seconds / window)
local elapsed_in_window = now_seconds - (current_window_id * window)

local stored = redis.call('HMGET', key, 'window_id', 'curr_count', 'prev_count')
local stored_window_id = tonumber(stored[1])
local curr_count = tonumber(stored[2]) or 0
local prev_count = tonumber(stored[3]) or 0

if stored_window_id == nil or stored_window_id < current_window_id - 1 then
    curr_count = 0
    prev_count = 0
elseif stored_window_id == current_window_id - 1 then
    prev_count = curr_count
    curr_count = 0
end

local overlap_ratio = (window - elapsed_in_window) / window
local estimated_count = curr_count + (prev_count * overlap_ratio)
local allowed = estimated_count < limit

local retry_after_ms = 0
if allowed then
    curr_count = curr_count + 1
else
    if curr_count >= limit or prev_count == 0 then
        retry_after_ms = math.ceil((window - elapsed_in_window) * 1000)
    else
        local elapsed_needed = window - (((limit - curr_count) * window) / prev_count)
        local wait_seconds = math.max(0, elapsed_needed - elapsed_in_window)
        retry_after_ms = math.ceil(wait_seconds * 1000)
    end
end

redis.call('HMSET', key, 'window_id', current_window_id, 'curr_count', curr_count, 'prev_count', prev_count)
redis.call('EXPIRE', key, window * 2)

local remaining = math.max(0, math.floor(limit - curr_count - (prev_count * overlap_ratio)))

local allowed_flag = 0
if allowed then
    allowed_flag = 1
end

return { allowed_flag, remaining, retry_after_ms }

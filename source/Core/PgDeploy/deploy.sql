-- extensions -----------------------------------
create extension if not exists "uuid-ossp";
create extension if not exists "pgcrypto";

-- USER ----------------------------------------
create table if not exists profile
(
    user_id uuid primary key,
    data jsonb not null
);

create table if not exists profile_photo
(
    user_id uuid primary key,
    photo_base64 text not null
);

-- AUTH -----------------------------------------
create table if not exists auth_data
(
    login text primary key,
    password_hash bytea not null,
    user_id uuid not null
);

create table if not exists auth_session
(
    token text primary key,
    user_id uuid not null
);

-- SOLUTION -------------------------------------
-------------------------------------------------
create table if not exists solution
(
    id uuid primary key,
    data jsonb not null
);

create table if not exists solution_rate
(
    solution_id uuid primary key,
    data jsonb not null
);

create table if not exists solution_index
(
    task_id uuid not null,
    solution_id uuid not null
);
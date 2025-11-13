-- Quick diagnostic query to check test account
USE l1j_remastered;

-- Check if account exists and its details
SELECT
    login AS 'Username',
    password AS 'Password',
    access_level AS 'Access Level',
    banned AS 'Banned',
    lastactive AS 'Last Active',
    ip AS 'Last IP',
    CHAR_LENGTH(password) AS 'Password Length'
FROM accounts
WHERE login = 'test';

-- If no results, account doesn't exist
-- Create test account if needed:
-- INSERT INTO accounts (login, password, access_level, ip, host, banned, lastactive)
-- VALUES ('test', 'test123', 0, '', '', 0, NOW());
